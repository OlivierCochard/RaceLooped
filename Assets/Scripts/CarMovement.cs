using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Movement")]
    public float frontMaxSpeedOutside = 1f;
    public float backMaxSpeedOutside = 0.75f;

    public float backMaxSpeed = 4f;
    public float frontMaxSpeed = 8f;
    public float acceleration = 20f;
    public float accelerationBack = 10f;
    public float decelerationManual = 30f;
    public float decelerationNatural = 5f;
    public LayerMask circuitLayer;

    [Header("Rotation")]
    public float turnSpeedMin = 5f;
    public float turnSpeedMax = 5f;
    public float minSpeedToRotate = 0.2f;

    [Header("Jump")]
    public float velocityReduction = 1.5f;

    [Header("Contrôles")]
    public KeyCode[] accelerationKeys;
    public KeyCode[] decelerationKeys;
    public KeyCode[] rightKeys;
    public KeyCode[] leftKeys;

    [Header("Particles")]
    public int particleMaxAmount;
    public int particleMinAmount;

    private bool isInside;
    private Rigidbody2D rb;
    private ParticleSystem ps;
    private AudioSource audioSource;
    private int speedDirection;
    private int turnDirection;
    private bool isJumping; 
    private Vector2 beforeJumpVelocity;


    public void SetIsJumping(bool isJumping){
        this.isJumping = isJumping;

        if (isJumping){
            beforeJumpVelocity = rb.linearVelocity;
            rb.linearVelocity = beforeJumpVelocity/velocityReduction;
        }
        else {
            rb.linearVelocity = beforeJumpVelocity;
        }
    }
    public bool GetIsJumping(){ return isJumping; }

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        ps = GetComponentInChildren<ParticleSystem>();
    }
    public void DestroyPlayer(){
        Destroy(gameObject);
    }
    void Update(){
        speedDirection = GetInputSpeed();

        turnDirection = GetInputTurn();
        isInside = IsInside();
    }
    void FixedUpdate(){
        Vector2 forward = transform.up;
        float currentSpeed = Mathf.Abs(Vector2.Dot(rb.linearVelocity, forward));
        audioSource.volume = (currentSpeed/frontMaxSpeed) * (PlayerPrefs.GetInt("Sounds", 50)/100f);
        if (isJumping) return;

        //Movement
        if (speedDirection > 0){
            ApplyFront();
        }
        else if (speedDirection < 0){
            ApplyBack();
            var emission = ps.emission;
            emission.rateOverTime = 0;
        }
        else{
            ApplyNothing();
            var emission = ps.emission;
            emission.rateOverTime = 0;
        }

        //IsInside
        if (isInside == false){
            ApplyOutside();
        }

        //Rotation
        if (turnDirection > 0){
            ApplyRight();
        }
        else if (turnDirection < 0){
            ApplyLeft();
        }
    }

    void ApplyNothing(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward); // vitesse dans la direction de la voiture
        float newSpeed = Mathf.MoveTowards(currentSpeed, 0, decelerationNatural * Time.fixedDeltaTime);
        rb.linearVelocity = forward * newSpeed;
    }
    void ApplyFront(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward);

        var emission = ps.emission;
        if (currentSpeed >= 0){
            float newSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, frontMaxSpeed);
            rb.linearVelocity = forward * newSpeed;

            float particleAmount = (newSpeed*particleMaxAmount/frontMaxSpeed);
            if (particleAmount < particleMinAmount) particleAmount = particleMinAmount;
            emission.rateOverTime = particleAmount;
        }
        else{
            float newSpeed = Mathf.MoveTowards(currentSpeed, 0, decelerationManual * Time.fixedDeltaTime);
            rb.linearVelocity = forward * newSpeed;
            emission.rateOverTime = particleMinAmount;
        }
    }
    void ApplyBack(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward);

        if (currentSpeed > 0){
            float newSpeed = Mathf.MoveTowards(currentSpeed, 0, decelerationManual * Time.fixedDeltaTime);
            rb.linearVelocity = forward * newSpeed;
        }
        else{
            float newSpeed = Mathf.Max(currentSpeed - accelerationBack * Time.fixedDeltaTime, -backMaxSpeed);
            rb.linearVelocity = forward * newSpeed;
        }
    }

    void ApplyRight(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward);

        if (currentSpeed == 0) return;
        float maxSpeed = frontMaxSpeed;
        if (currentSpeed < 0) maxSpeed = -backMaxSpeed;

        float speed = (currentSpeed - minSpeedToRotate) / (maxSpeed - minSpeedToRotate);
        float turnSpeed = Mathf.Lerp(turnSpeedMax, turnSpeedMin, speed);

        float currentZ = transform.eulerAngles.z;
        float newZ = currentZ - turnSpeed;
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }
    void ApplyLeft(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward);

        if (currentSpeed == 0) return;
        float maxSpeed = frontMaxSpeed;
        if (currentSpeed < 0) maxSpeed = -backMaxSpeed;

        float speed = (currentSpeed - minSpeedToRotate) / (maxSpeed - minSpeedToRotate);
        float turnSpeed = Mathf.Lerp(turnSpeedMax, turnSpeedMin, speed);

        float currentZ = transform.eulerAngles.z;
        float newZ = currentZ + turnSpeed;
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    void ApplyOutside(){
        Vector2 forward = transform.up;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, forward);

        if (currentSpeed > frontMaxSpeedOutside){
            float newSpeed = Mathf.MoveTowards(currentSpeed, frontMaxSpeedOutside, decelerationManual * Time.fixedDeltaTime * 5);
            rb.linearVelocity = forward * newSpeed;
        }
        else if (currentSpeed < -backMaxSpeedOutside){
            float newSpeed = Mathf.MoveTowards(currentSpeed, -backMaxSpeedOutside, decelerationManual * Time.fixedDeltaTime * 5);
            rb.linearVelocity = forward * newSpeed;
        }
    }

    public int GetInputSpeed(){
        int res = 0;
        foreach (KeyCode keyCode in accelerationKeys){
            if (Input.GetKey(keyCode)) res++;
        }
        foreach (KeyCode keyCode in decelerationKeys){
            if (Input.GetKey(keyCode)) res--;
        }
        return res;
    }
    int GetInputTurn(){
        int res = 0;
        foreach (KeyCode keyCode in rightKeys){
            if (Input.GetKey(keyCode)) res++;
        }
        foreach (KeyCode keyCode in leftKeys){
            if (Input.GetKey(keyCode)) res--;
        }
        return res;
    }

    bool IsInside(){
        return Physics2D.OverlapCapsule(transform.position, new Vector2(0.375f, 0.3125f), CapsuleDirection2D.Horizontal, 0, circuitLayer);
    }
}
