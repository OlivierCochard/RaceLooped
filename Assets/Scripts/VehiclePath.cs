using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehiclePath : MonoBehaviour
{
    public float saveStep;
    float timer;

    PathData currentPathData;
    List<PathData> pathDataList = new List<PathData>();
    List<CollisionData> collisionsDataList = new List<CollisionData>();

    public void PrepareRound(float timer){
        this.timer = timer;
    }
    public void EndRound(){
        pathDataList.Add(currentPathData);
        currentPathData = null;
    }

    public void StartSave(Transform playerTransform){
        currentPathData = gameObject.AddComponent<PathData>();
        currentPathData.PrepareSaves(playerTransform, timer, saveStep);
    }
    public PathData GetPathData(int index){
        if (index >= pathDataList.Count) return null;
        return pathDataList[index];
    }

    public void AddCollision(float elapsedTime, Transform carTransform, int damage, Vector2 direction){
        PathData pathData = GetPathDataFromTransform(carTransform);
        if (pathData == null){
            return;
        }

        collisionsDataList.Add(new CollisionData(elapsedTime, pathData, damage, direction));
    }
    public void StartCollisions(){
        foreach (CollisionData collisionData in collisionsDataList){
            StartCoroutine(ApplyDamageAfterDelay(collisionData));
        }
    }

    private IEnumerator ApplyDamageAfterDelay(CollisionData collisionData){
        yield return new WaitForSeconds(collisionData.GetElapsedTime());
        collisionData.GetPathData().GetCarTransform().GetComponent<BotTrigger>().ApplyDamage(collisionData.GetDamage(), collisionData.GetDirection());
    }
    PathData GetPathDataFromTransform(Transform carTransform){
        if (currentPathData.GetCarTransform() == carTransform) return currentPathData;
        foreach (PathData pathData in pathDataList){
            if (pathData.GetCarTransform() == carTransform) return pathData;
        }
        return null;
    }

    public void UpdateTransform(int botNumero, Transform carTransform){
        pathDataList[botNumero].UpdateTransform(carTransform);
    }
}

public class CollisionData
{
    float elapsedTime;
    PathData pathData;
    Vector2 direction;
    int damage;

    public CollisionData(float elapsedTime, PathData pathData, int damage, Vector2 direction){
        this.elapsedTime = elapsedTime;
        this.direction = direction;
        this.pathData = pathData;
        this.damage = damage;
    }

    public float GetElapsedTime(){ return elapsedTime; }
    public PathData GetPathData(){ return pathData; }
    public Vector2 GetDirection(){ return direction; }
    public int GetDamage(){ return damage; }
}
