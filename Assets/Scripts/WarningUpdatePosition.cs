using UnityEngine;

public class WarningUpdatePosition : MonoBehaviour
{
    public Vector3 pos;
    Transform parentTransform;

    void OnEnable(){
        parentTransform = transform.parent;
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    void Update(){
        transform.position = parentTransform.position + pos;
    }
}
