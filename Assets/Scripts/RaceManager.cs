using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public float timeBetweenSpawn;
    public float timeBetweenRound;
    public int startingRotation;
    public int timer;
    public float penaltyEachEnd;
    public Transform[] spawnPosition;
    public DrivingBoardManager[] drivingBoardManagerArray;

    public GameObject playerDeathPrefab;
    public GameObject botDeathPrefab;

    CameraManager cameraManager;
    float penalty;
    int roundNumber;
    bool hasRoundStarted;
    GameObject playerObject;
    List<GameObject> botObjects;
    MenuManager menuManager;
    VehiclePath vehiclePath;
    int entityWinAmount;

    int botNumero;
    bool hasLost;
    bool hasWin;
    SfxManager sfxManager;

    float accumulatedTime;
    float startTime;

    public List<GameObject> GetBotObjects(){ return botObjects; }
    public GameObject GetPlayerObject(){ return playerObject; }

    void Awake(){
        GameObject sfxManagerObject = GameObject.Find("SfxManager");
        cameraManager = GameObject.Find("Main Camera").GetComponent<CameraManager>();
        if (sfxManagerObject != null){
            sfxManager = sfxManagerObject.GetComponent<SfxManager>();
        }
    }
    void Start(){
        vehiclePath = GetComponent<VehiclePath>();
        menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
        PrepareRound();
    }
    void Update(){
        accumulatedTime += Time.deltaTime;
        if (hasRoundStarted || playerObject == null || playerObject.GetComponent<CarMovement>() == null || playerObject.GetComponent<CarMovement>().GetInputSpeed() == 0) return;

        StartRound();
    }

    public int GetRound(){ return roundNumber; }

    public void AddCollision(Transform botTransform, int damage, Vector2 direction){
        float elapsedTime = accumulatedTime - startTime;
        vehiclePath.AddCollision(elapsedTime, playerObject.transform, damage, direction);

        if (botTransform != null) vehiclePath.AddCollision(elapsedTime, botTransform, damage, direction);
    }

    void SummonWaves(){
        vehiclePath.StartCollisions();
        startTime = accumulatedTime;

        int tmp = roundNumber;
        int delay = 0;
        while (tmp >= 0){
            tmp -= spawnPosition.Length;
            SummonWave(delay, tmp < 0);
            delay++;
        }
        penalty = (delay-1) * penaltyEachEnd;
    }
    void SummonWave(int delay, bool containsPlayer){
        int start = 0;
        for (int i = 0; i < delay; i++){
            start += spawnPosition.Length;
        }

        int end = roundNumber;
        while (end - start >= spawnPosition.Length){
            end = start + spawnPosition.Length-1;
        }

        for (int i = start; i <= end; i++){
            if (containsPlayer && i == roundNumber) Invoke("PreparePlayer", timeBetweenSpawn*delay);
            else Invoke("PrepareBot", timeBetweenSpawn*delay);
        }
    }
    void PrepareRound(){
        hasWin = false;
        entityWinAmount = 0;
        vehiclePath.PrepareRound(timer);
        botObjects = new List<GameObject>();

        botNumero = -1;
        SummonWaves();
        roundNumber++;
        if (cameraManager != null) cameraManager.ResetCameraPos();
    }
    void StartRound(){
        hasRoundStarted = true;
        menuManager.Resume();
    }
    void EndRound(){
        CancelInvoke("EndRound");
        if (hasWin == false){
            Lose();
            return;
        }
        Win();
    }

    void PreparePlayer(){
        playerObject = Instantiate(playerPrefab);
        int tmp = roundNumber-1;
        while (tmp > spawnPosition.Length-1){
            tmp -= spawnPosition.Length;
        }
        playerObject.transform.position = spawnPosition[tmp].position;
        playerObject.transform.eulerAngles = new Vector3(0, 0, startingRotation);

        hasRoundStarted = false;
        menuManager.ActiveWaitingMenu(penalty);
        vehiclePath.StartSave(playerObject.transform);

        Invoke("EndRound", timer - penalty);
        menuManager.StartTimerBar(timer - penalty);
        sfxManager.RefreshVolumeCar(this);

        if (cameraManager != null) cameraManager.SetPlayerTransform(playerObject.transform);

        if (drivingBoardManagerArray == null) return;
        foreach (DrivingBoardManager dbm in drivingBoardManagerArray){
            dbm.Set(true);
        }
        
    }
    void GhostPlayer(){
        playerObject.tag = "Ghost";
        playerObject.layer = LayerMask.NameToLayer("Ghost");
        playerObject.GetComponent<Animator>().SetTrigger("Ghost");
        Destroy(playerObject.GetComponent<CarTrigger>().warningObject);
    }
    void DestroyPlayer(){
        if (playerObject == null || playerObject.GetComponent<CarMovement>() == null) return;
        playerObject.GetComponent<CarMovement>().DestroyPlayer();
    }

    void PrepareBot(){
        botNumero++;
        int tmp = botNumero;
        while (tmp > spawnPosition.Length-1){
            tmp -= spawnPosition.Length;
        }

        GameObject botObject = Instantiate(botPrefab);
        botObject.transform.position = spawnPosition[tmp].position;
        botObject.transform.eulerAngles = new Vector3(0, 0, startingRotation);
        botObjects.Add(botObject);

        sfxManager.RefreshVolumeCar(this);
        vehiclePath.UpdateTransform(botNumero, botObject.transform);
        BotMovement botMovement = botObject.AddComponent<BotMovement>();
        botMovement.StartBot(botObject.transform, timer, vehiclePath.saveStep, vehiclePath.GetPathData(botNumero));
    }
    void GhostBots(){
        foreach (GameObject botObject in botObjects){
            GhostBot(botObject);
        }
    }
    void GhostBot(GameObject botObject){
        if (botObject == null) return;
        botObject.tag = "Ghost";
        botObject.layer = LayerMask.NameToLayer("Ghost");
        botObject.GetComponent<Animator>().SetTrigger("Ghost");
        Destroy(botObject.GetComponent<BotTrigger>().warningObject);
    }
    void DestroyBots(){
        foreach (GameObject botObject in botObjects){
            if (botObject != null) Destroy(botObject);
        }
    }

    public void KillPlayer(Vector2 direction){
        hasLost = true;
        menuManager.StopTimerBar();
        Invoke("Lose", timeBetweenRound);
        foreach (GameObject botObject in botObjects){
            if (botObject != null) botObject.GetComponent<BotMovement>().enabled = false;
        }

        Vector3 pos = playerObject.transform.position;
        Vector3 rot = playerObject.transform.eulerAngles;
        Destroy(playerObject);

        GameObject obj = Instantiate(playerDeathPrefab);
        obj.transform.position = pos;
        obj.transform.eulerAngles = rot;
        Transform child = obj.transform.GetChild(0);
        Vector2 directionRight = new Vector2(direction.y, -direction.x + direction.x/2);

        child.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direction/2, ForceMode2D.Impulse);
        child.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        child.GetChild(2).GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        child.GetChild(3).GetComponent<Rigidbody2D>().AddForce(directionRight, ForceMode2D.Impulse);
        child.GetChild(4).GetComponent<Rigidbody2D>().AddForce(directionRight, ForceMode2D.Impulse);
        child.GetChild(3).GetComponent<Rigidbody2D>().AddForce(-directionRight, ForceMode2D.Impulse);
        child.GetChild(4).GetComponent<Rigidbody2D>().AddForce(-directionRight, ForceMode2D.Impulse);
    }
    public void KillBot(GameObject botObject, Vector2 direction){
        hasLost = true;
        menuManager.StopTimerBar();
        Invoke("Lose", timeBetweenRound);
        if (playerObject != null) playerObject.GetComponent<CarMovement>().enabled = false;

        Vector3 pos = botObject.transform.position;
        Vector3 rot = botObject.transform.eulerAngles;
        Destroy(botObject);

        GameObject obj = Instantiate(botDeathPrefab);
        obj.transform.position = pos;
        obj.transform.eulerAngles = rot;
        Transform child = obj.transform.GetChild(0);
        Vector2 directionRight = new Vector2(direction.y, -direction.x + direction.x/2);

        child.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direction/2, ForceMode2D.Impulse);
        child.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        child.GetChild(2).GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        child.GetChild(3).GetComponent<Rigidbody2D>().AddForce(directionRight, ForceMode2D.Impulse);
        child.GetChild(4).GetComponent<Rigidbody2D>().AddForce(directionRight, ForceMode2D.Impulse);
        child.GetChild(3).GetComponent<Rigidbody2D>().AddForce(-directionRight, ForceMode2D.Impulse);
        child.GetChild(4).GetComponent<Rigidbody2D>().AddForce(-directionRight, ForceMode2D.Impulse);
    }
    
    void CheckTotalWin(){
        entityWinAmount++;
        if (entityWinAmount == roundNumber){
            CancelInvoke("EndRound");
            EndRound();
        }
    }
    public void BotWin(GameObject botObject){
        GhostBot(botObject);
        CheckTotalWin();

        sfxManager.AddSoundsSource("finishBot");
    }
    public void PlayerWin(){
        if (hasRoundStarted == false || hasLost) return;
        hasRoundStarted = true;

        GhostPlayer();
        hasWin = true;
        CheckTotalWin();
        menuManager.ActiveRoundMenu();
        menuManager.StopTimerBar();

        sfxManager.AddSoundsSource("win");
    }

    void Win(){
        vehiclePath.EndRound();
        Invoke("PrepareRound", timeBetweenRound);
    }
    void Lose(){
        menuManager.ActiveDeathMenu();
    }
}
