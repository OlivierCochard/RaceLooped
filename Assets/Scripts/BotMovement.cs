using UnityEngine;

public class BotMovement : MonoBehaviour
{
    [SerializeField] private int particleMaxAmount = 50;
    [SerializeField] private int particleMinAmount = 10;
    [SerializeField] private float interpolationSpeed = 5f;

    private AudioSource audioSource;
    private ParticleSystem ps;
    private PathData pathData;
    private Transform carTransform;
    private Rigidbody2D rb;
    private int step;
    private Vector2 targetVelocity;
    private Vector2 targetPosition;
    private float targetRotation;
    private bool isActive;
    private float saveStep;
    bool isJumping;

    public void SetIsJumping(bool isJumping){
        if (this.isJumping == isJumping) return;
        
        this.isJumping = isJumping;
        GetComponent<Animator>().SetBool("IsJumping", isJumping);
    }

    public void StartBot(Transform carTransform, float timer, float saveStep, PathData pathData)
    {
        this.carTransform = carTransform;
        this.pathData = pathData;
        this.saveStep = saveStep;

        rb = carTransform.GetComponent<Rigidbody2D>();
        audioSource = carTransform.GetComponent<AudioSource>();
        ps = carTransform.GetComponentInChildren<ParticleSystem>();

        isActive = true;
        step = 0;
        UpdateTargetValues();
        
        // Utilisation de Update au lieu de InvokeRepeating pour un meilleur contrôle
        Invoke("CancelRefresh", timer);
    }

    void Update()
    {
        if (!isActive) return;

        // Interpolation pour des mouvements plus fluides
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * interpolationSpeed);
        carTransform.position = Vector2.Lerp(carTransform.position, targetPosition, Time.deltaTime * interpolationSpeed);
        
        float currentAngle = carTransform.eulerAngles.z;
        float deltaAngle = Mathf.DeltaAngle(currentAngle, targetRotation);
        carTransform.eulerAngles = new Vector3(0, 0, currentAngle + deltaAngle * Time.deltaTime * interpolationSpeed);

        UpdateParticlesAndAudio();
    }

    void FixedUpdate()
    {
        if (!isActive) return;
        
        // Passage à l'étape suivante à intervalle régulier
        if (Time.fixedTime % saveStep < Time.fixedDeltaTime)
        {
            step++;
            if (step >= pathData.GetVelocitys().Count)
            {
                CancelRefresh();
                return;
            }
            UpdateTargetValues();
        }
    }

    void UpdateTargetValues()
    {
        targetVelocity = pathData.GetVelocitys()[step];
        targetPosition = pathData.GetPositions()[step];
        targetRotation = pathData.GetRotations()[step];
        SetIsJumping(pathData.GetIsJumpings()[step]);
    }

    void UpdateParticlesAndAudio()


    {
        var emission = ps.emission;
        bool front = pathData.GetFronts()[step];
        
        if (front)
        {
            Vector2 forward = transform.up;
            float currentSpeed = Mathf.Abs(Vector2.Dot(rb.linearVelocity, forward));
            float volume = (currentSpeed/6f) * (PlayerPrefs.GetInt("Sounds", 50)/100f);
            
            if (currentSpeed >= 0)
            {
                // Calcul des particules avec lissage
                float particleAmount = Mathf.Lerp(emission.rateOverTime.constant, 
                    Mathf.Max(currentSpeed * particleMaxAmount / 6f, particleMinAmount), 
                    Time.deltaTime * interpolationSpeed);
                emission.rateOverTime = particleAmount;

                // Calcul de l'audio avec lissage
                float audioAmount = Mathf.Lerp(audioSource.volume, 
                    volume, 
                    Time.deltaTime * interpolationSpeed);
                audioSource.volume = audioAmount * (PlayerPrefs.GetInt("Sounds", 50) / 100f);
            }
            else 
            {
                emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, particleMinAmount, Time.deltaTime * interpolationSpeed);
            }
        }
        else 
        {
            emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, 0, Time.deltaTime * interpolationSpeed);
        }
    }

    void CancelRefresh()
    {
        isActive = false;
        CancelInvoke("CancelRefresh");
    }
}