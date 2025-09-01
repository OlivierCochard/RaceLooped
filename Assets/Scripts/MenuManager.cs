using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public int levelNumero;
    public RaceManager raceManager;
    public GameObject genericMenuObject;
    public GameObject resumeButtonObject;
    public GameObject restartButtonObject;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI roundText_generic;
    public RawImage backgroundImage;
    public GameObject waitingMenuObject;
    public TextMeshProUGUI roundText_waiting;
    public GameObject roundMenuObject;
    public TextMeshProUGUI roundText_round;
    public GameObject settingsMenuObject;
    public TextMeshProUGUI musicTextButton;
    public TextMeshProUGUI soundsTextButton;
    public GameObject backButtonObject_Death;
    public GameObject backButtonObject_Pause;
    public Slider slider;

    public Image imageBackKmh;
    public TextMeshProUGUI textKmh;
    public Color colorPositiveSpeed;
    public Color colorNegativeSpeed;
    public float speedMultiplicatorKmh;


    bool timing;
    float timer;
    SfxManager sfxManager;

    void Awake(){
        Cursor.visible = false;
        GameObject sfxManagerObject = GameObject.Find("SfxManager");
        if (sfxManagerObject != null) sfxManager = sfxManagerObject.GetComponent<SfxManager>();
        if (sfxManager != null){
            sfxManager.RefreshVolume();
            sfxManager.RefreshVolumeCar(raceManager);
        }
    }
    void Update(){
        if (timing && timer > 0){
            timer -= Time.deltaTime * Time.timeScale;
            timer = Mathf.Max(0, timer);
            slider.value = timer;

            if (raceManager != null){
                GameObject playerObject = raceManager.GetPlayerObject();
                if (playerObject != null){
                    Vector2 forward = playerObject.transform.up;
                    float currentSpeed = Vector2.Dot(playerObject.GetComponent<Rigidbody2D>().linearVelocity, forward);
                    imageBackKmh.color = colorPositiveSpeed;

                    float maxSpeed = 6;
                    string addString = "";
                    if (currentSpeed < 0){
                        currentSpeed*=-1;
                        imageBackKmh.color = colorNegativeSpeed;
                        addString += "(R)";
                        maxSpeed = 4;
                    }
                    textKmh.text = "" + (int)(currentSpeed * speedMultiplicatorKmh) + addString;

                    imageBackKmh.fillAmount = currentSpeed/maxSpeed;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)){
            if (roundMenuObject.activeSelf || waitingMenuObject.activeSelf) return;

            if (genericMenuObject.activeSelf) Resume();
            else ActivePauseMenu();
        }
    }

    void UnactiveAll(GameObject objMenu){
        roundMenuObject.SetActive(false);
        genericMenuObject.SetActive(false);
        waitingMenuObject.SetActive(false);
        resumeButtonObject.SetActive(false);
        restartButtonObject.SetActive(false);
        resumeButtonObject.SetActive(false);

        if (objMenu != null) objMenu.SetActive(true);
        else if (sfxManager != null) sfxManager.AddSoundsSource("notif");

        Cursor.visible = false;
    }

    public void StartTimerBar(float valMax){
        timer = valMax;
        timing = true;
        slider.maxValue  = valMax;
        slider.value = valMax;
    }
    public void StopTimerBar(){
        timing = false;
        textKmh.text = "" + 0;
    }

    int GetHighScore(){ return PlayerPrefs.GetInt("level_" + levelNumero, 0); }
    int GetScore(){ return raceManager.GetRound()-1; }
    void SetHighScore(){ PlayerPrefs.SetInt("level_" + levelNumero, GetScore()); }

    public void ActiveRoundMenu(){
        UnactiveAll(roundMenuObject);
        roundText_round.text = "Score " + (GetScore()+1);
    }
    public void ActiveWaitingMenu(float penalty){
        UnactiveAll(waitingMenuObject);
        roundText_waiting.text = "Round " + raceManager.GetRound();
        Time.timeScale = 0f;

        if (penalty != 0){
            roundText_waiting.text += " - Penalty " + penalty + "s";
        }
    }
    public void ActiveDeathMenu(){
        UnactiveAll(genericMenuObject);
        restartButtonObject.SetActive(true);
        backButtonObject_Pause.SetActive(false);
        backButtonObject_Death.SetActive(true);
        settingsMenuObject.SetActive(false);
        titleText.text = "You Lost";

        if (GetScore() <= GetHighScore()){
            roundText_generic.text = "Score " + GetScore() + " (" + GetHighScore() + ")";
        }
        else {
            roundText_generic.text = "Score " + GetScore() + " -> (highscore)";
            SetHighScore();
        }
        
        Time.timeScale = 0f;
        if (sfxManager != null){
            sfxManager.AddSoundsSource("lose");
            sfxManager.StopMusicSource();
        }

        Cursor.visible = true;
    }
    public void ActivePauseMenu(){
        UnactiveAll(genericMenuObject);
        resumeButtonObject.SetActive(true);
        backButtonObject_Death.SetActive(false);
        backButtonObject_Pause.SetActive(true);
        settingsMenuObject.SetActive(false);
        titleText.text = "You Paused";
        roundText_generic.text = "Score " + GetScore() + " (" + GetHighScore() + ")";
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    public void Resume(){
        Time.timeScale = 1f;
        UnactiveAll(null);
    }

    public void ActiveSettingsMenu(){
        UnactiveAll(settingsMenuObject);
        RefreshSettings();
        Cursor.visible = true;
    }
    public void RefreshSettings(){
        musicTextButton.text = "MUSIC\n" + PlayerPrefs.GetInt("Music", 50);
        soundsTextButton.text = "SOUNDS\n" + PlayerPrefs.GetInt("Sounds", 50);
        Awake();
        Cursor.visible = true;
    }
    public void AddMusic(){
        ModifyVolume("Music", 5);
    }
    public void LessMusic(){
        ModifyVolume("Music", -5);
    }
    public void AddSounds(){
        ModifyVolume("Sounds", 5);
    }
    public void LessSounds(){
        ModifyVolume("Sounds", -5);
    }
    void ModifyVolume(string type, int amount){
        PlayerPrefs.SetInt(type, PlayerPrefs.GetInt(type, 50) + amount);
        if (PlayerPrefs.GetInt(type, 50) > 100) PlayerPrefs.SetInt(type, 100);
        if (PlayerPrefs.GetInt(type, 50) < 0) PlayerPrefs.SetInt(type, 0);
        PlayerPrefs.Save();

        RefreshSettings();
    }

    public void MainMenu(){
        SceneManager.LoadScene("menu");
    }
    public void ResetLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }   
}