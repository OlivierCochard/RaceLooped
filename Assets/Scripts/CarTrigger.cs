using UnityEngine;

public class CarTrigger : MonoBehaviour
{
    public GameObject warningObject;
    public int lifeAmount;
    public float impactMax;
    public float impactMini;
    public float immortalDelay = 0.5f;
    RaceManager raceManager;
    bool immortal;
    SfxManager sfxManager;

    void Awake(){
        raceManager = GameObject.Find("MAIN").GetComponent<RaceManager>();
        GameObject sfxManagerObject = GameObject.Find("SfxManager");
        if (sfxManagerObject != null) sfxManager = sfxManagerObject.GetComponent<SfxManager>();
    }

    void OnTriggerEnter2D(Collider2D col){
        if (col.tag == "Finish"){
            raceManager.PlayerWin();
            immortal = true;
        }

        if (col.tag == "Start"){
            col.transform.parent.GetComponent<DrivingBoardManager>().ApplyStart();
        }
        if (col.tag == "End"){
            col.transform.parent.GetComponent<DrivingBoardManager>().ApplyEnd();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.collider.CompareTag("Bot") || collision.collider.CompareTag("Wall")){
            float impactForce = collision.relativeVelocity.magnitude;

            int damage = 1;
            ContactPoint2D contact = collision.contacts[0];
            Vector2 direction = (transform.position - (Vector3)contact.point).normalized;
            if (impactForce > impactMini && !gameObject.CompareTag("Ghost") && collision.contactCount > 0 && immortal == false){
                if (impactForce > impactMax) damage = 2;
                ApplyDamage(damage, direction);

                Transform botTransform = null;
                if (collision.collider.CompareTag("Bot")) botTransform = collision.transform;
                raceManager.AddCollision(botTransform, damage, direction);
            }
        }
    }

    void ApplyDamage(int damage, Vector2 direction){
        sfxManager.AddSoundsSource("colliderprojectile");
        lifeAmount -= damage;
        immortal = true;
        Invoke("StopImmortal", immortalDelay);
        if (lifeAmount <= 0){
            raceManager.KillPlayer(direction);
            Destroy(warningObject);
        }
        else warningObject.SetActive(true);
    }

    void StopImmortal(){
        immortal = false;
    }
}
