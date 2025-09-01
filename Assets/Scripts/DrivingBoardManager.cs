using UnityEngine;

public class DrivingBoardManager : MonoBehaviour
{
    public RaceManager raceManager;
    public Collider2D startCollider;
    public Collider2D endCollider;
    public float durationMin = 0.5f;
    public float divideBy;

    public void ApplyStart(){
        Vector2 forward = raceManager.GetPlayerObject().transform.up;
        float jumpDuration = (Mathf.Abs(Vector2.Dot(raceManager.GetPlayerObject().GetComponent<Rigidbody2D>().linearVelocity, forward)) / divideBy);
        Invoke("ApplyEnd", jumpDuration);

        Set(false);
    }
    public void ApplyEnd(){
        CancelInvoke("ApplyEnd");
        Set(true);
    }

    public void Set(bool boolean){
        raceManager.GetPlayerObject().GetComponent<CarMovement>().SetIsJumping(!boolean);
        raceManager.GetPlayerObject().GetComponent<Animator>().SetBool("IsJumping", !boolean);
        startCollider.enabled = boolean;
        endCollider.enabled = boolean;
    }
}
