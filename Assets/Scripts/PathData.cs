using UnityEngine;
using System.Collections.Generic;

public class PathData : MonoBehaviour
{   
    List<bool> isJumpingSaves;
    List<Vector2> velocitySaves;
    List<Vector2> positionSaves;
    List<float> rotationSaves;
    List<bool> frontSaves;
    
    Rigidbody2D rb;
    Transform carTransform;
    
    public void PrepareSaves(Transform carTransform, float timer, float saveStep){
        this.carTransform = carTransform;

        rb = carTransform.GetComponent<Rigidbody2D>();

        isJumpingSaves = new List<bool>();
        velocitySaves = new List<Vector2>();
        positionSaves = new List<Vector2>();
        rotationSaves = new List<float>();
        frontSaves = new List<bool>();

        InvokeRepeating("Save", saveStep, saveStep);
        Invoke("CancelSave", timer);
    }

    public List<bool> GetIsJumpings(){ return isJumpingSaves; }
    public List<Vector2> GetVelocitys(){ return velocitySaves; }
    public List<Vector2> GetPositions(){ return positionSaves; }
    public List<float> GetRotations(){ return rotationSaves; }
    public List<bool> GetFronts(){ return frontSaves; }

    void Save(){
        if (carTransform == null || rb == null || carTransform.GetComponent<CarMovement>() == null) return;

        isJumpingSaves.Add(carTransform.GetComponent<CarMovement>().GetIsJumping());
        rotationSaves.Add(carTransform.eulerAngles.z);
        velocitySaves.Add(rb.linearVelocity);
        positionSaves.Add(carTransform.transform.position);
        frontSaves.Add(carTransform.GetComponent<CarMovement>().GetInputSpeed() == 1);
    }
    public void CancelSave(){
        CancelInvoke("CancelSave");
        CancelInvoke("Save");
        Save();
    }

    public void UpdateTransform(Transform carTransform){
        this.carTransform = carTransform;
    }
    public Transform GetCarTransform(){ return carTransform; }
}
