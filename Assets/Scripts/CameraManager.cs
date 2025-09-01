using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Vector2 xPos;
    public Vector2 yPos;
    Transform playerTransform;
    Camera camera;

    void Awake(){
        camera = GetComponent<Camera>();
    }
    void Update(){
        if (playerTransform == null) return;

        Vector2 newPlayerPos = Vector2.zero;
        Vector3 playerPos = playerTransform.position;
        if (playerPos.x > xPos.x && playerPos.x < xPos.y){
            newPlayerPos.x = playerPos.x;
        }
        else if (playerPos.x <= xPos.x){
            newPlayerPos.x = xPos.x;
        }
        else if (playerPos.x >= xPos.y){
            newPlayerPos.x = xPos.y;
        }
        if (playerPos.y > yPos.x && playerPos.y < yPos.y){
            newPlayerPos.y =playerPos.y;
        }
        else if (playerPos.y <= yPos.x){
            newPlayerPos.y = yPos.x;
        }
        else if (playerPos.y >= yPos.y){
            newPlayerPos.y = yPos.y;
        }
        camera.transform.position = new Vector3(newPlayerPos.x, newPlayerPos.y, -10);
    }
    public void ResetCameraPos(){
        camera.transform.position = new Vector3(xPos.x, yPos.x, -10);
    }
    public void SetPlayerTransform(Transform playerTransform){
        this.playerTransform = playerTransform;
    }
}
