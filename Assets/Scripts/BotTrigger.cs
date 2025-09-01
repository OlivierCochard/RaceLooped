using UnityEngine;

public class BotTrigger : MonoBehaviour
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
            raceManager.BotWin(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.collider.CompareTag("Player")){
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce > impactMini && !gameObject.CompareTag("Ghost") && collision.contactCount > 0){
                ContactPoint2D contact = collision.contacts[0];
                Vector2 direction = (transform.position - (Vector3)contact.point).normalized;
                if (impactForce > impactMax) ApplyDamage(2, direction);
                else ApplyDamage(1, direction);
            }
        }
    }

    public void ApplyDamage(int damage, Vector2 direction){
        if (immortal) return;

        sfxManager.AddSoundsSource("colliderprojectile");
        lifeAmount --;
        immortal = true;
        Invoke("StopImmortal", immortalDelay);
        if (lifeAmount <= 0){
            raceManager.KillBot(gameObject, direction);
            Destroy(warningObject);
        }
        else warningObject.SetActive(true);
    }

    void StopImmortal(){
        immortal = false;
    }
    void Destroy(){
        Destroy(gameObject);
    }
}
