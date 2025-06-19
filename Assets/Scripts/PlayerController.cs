using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float rotationSpeed = 600f;
    [SerializeField] float acceleration = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    [Header("Push Settings")]
    [SerializeField] float pushForce = 10f;
    [SerializeField] float pushSpeedMultiplier = 0.3f;
    private GameObject currentPushableObject;
    private bool isPushing = false;

    [Header("Step Climbing Settings")]
    [SerializeField] float stepHeight = 0.35f;
    [SerializeField] float stepDistance = 0.5f;
    [SerializeField] float stepForce = 2.1f;

    [Header("Audio")]
    [Tooltip("Suono da riprodurre quando il giocatore si nasconde/mostra")]
    public AudioClip hideToggleSound;
    
    [Header("UI Elements")]
    public GameObject hidePrompt;

    // Public States
    public bool IsStealth = false;
    public static bool IsHidden = false;
    public bool IsMoving = false;
    public bool IsDead = false;
    public bool CanMove = true;
    public bool RedKey = false;
    public bool BlueKey = false;

    // Private Components
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Animator animator;
    private CameraController cameraController;
    private AudioSource audioSource;
    private GameObject graphics;

    // Private States
    private bool isGrounded;

    private bool isHidable = false;


    // Movement Variables
    private Quaternion targetRotation;
    private Vector3 currentVelocity;
    private float h, v;
    private Vector3 moveDir;
    private float moveAmount;
    private float lastStepTime;
    private bool prevStealth = false;

    private void Awake()
    {
        InitializeComponents();
        SetupRigidbody();

        if (hidePrompt != null)
            hidePrompt.SetActive(true);
            hidePrompt.SetActive(false);
    }

    private void InitializeComponents()
    {
        // mesh e componenti grafici
        graphics = transform.GetChild(0).gameObject;
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<CameraController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void SetupRigidbody()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        // gestione audio passi
        StepAudio.PlayerCanMove = CanMove;
        StepAudio.PlayerIsHidden = IsHidden;
        
        if (IsDead || !CanMove)
        {
            HandleDeadState();
            return;
        }

        HandleHideToggle();

        if (!IsHidden)
        {
            CacheInput();
            HandleStealthToggle();
            HandleRotation();
            UpdateAnimation();
        }
    }

    private void FixedUpdate()
    {
        if (IsDead || !CanMove || IsHidden)
            return;

        GroundCheck();
        HandleMovement();
        HandleStepClimbing();
        HandlePush();
    }

    private void HandleDeadState()
    {
        animator.SetFloat("moveAmount", 0f, 0f, Time.deltaTime);
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        currentVelocity = Vector3.zero;
    }

    private void CacheInput()
    {
        //lettura degli assi
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);

        //se l'input è maggiore di 1, vuol dire che va in diagonale, quindi lo normalizziamo
        if (input.magnitude > 1f)
            input = input.normalized;

        // Calcola la direzione di movimento e la quantità di movimento in base all'orientamento della camera
        moveDir = cameraController.PlanarRotation * input;
        moveAmount = input.magnitude;

        // Se sta spingendo, limita il movimento solo in avanti
        if (isPushing && currentPushableObject != null)
        {
            float forwardDot = Vector3.Dot(moveDir, transform.forward);
            forwardDot = Mathf.Max(0f, forwardDot);
            moveDir = transform.forward * forwardDot;
            moveAmount = forwardDot;
        }
    }

    private void HandleStealthToggle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            IsStealth = !IsStealth;
            animator.SetBool("IsStealth", IsStealth);
        }
    }

    private void HandleHideToggle()
    {
        if (Input.GetKeyDown(KeyCode.E) && isHidable)
        {
            IsHidden = !IsHidden;
            PlayHideToggleSound();
            
            rb.useGravity = !IsHidden;
            capsuleCollider.enabled = !IsHidden;
            graphics.SetActive(!IsHidden);
            hidePrompt.SetActive(!IsHidden && isHidable);

            if (IsHidden)
            {
                rb.linearVelocity = Vector3.zero;
                currentVelocity = Vector3.zero;
            }
        }
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("moveAmount", moveAmount, 0.1f, Time.deltaTime);
        IsMoving = moveAmount > 0.01f;
    }

    private void HandleMovement()
    {
        //due velocità uno aper Stealth una per camminata normale
        float baseSpeed = IsStealth ? walkSpeed : runSpeed;
        Vector3 targetVelocity;

        if (isPushing)
        {
            float pushSpeed = runSpeed * pushSpeedMultiplier;
            targetVelocity = moveDir * pushSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            targetVelocity = moveDir * baseSpeed;
            
            float accelerationMultiplier = 1f;
            if (moveAmount > 0.1f && currentVelocity.magnitude < targetVelocity.magnitude * 0.7f)
            {
                // per gestire l'interruzione della spinta
                accelerationMultiplier = 2.5f;
            }
            
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * accelerationMultiplier * Time.fixedDeltaTime);
        }
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void HandleRotation()
    {
        if (moveAmount <= 0.01f || isPushing) return;
        
        // calcola una rotazione che guarda esattamente nella direzione del movimento
        targetRotation = Quaternion.LookRotation(moveDir);
        // rotazione del rigidbody
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));
    }

    private void GroundCheck()
    {
        // capiamo se il giocatore è sulle scale
        // groundLayer -> Obstacles ovvero scale
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
    }

    private void HandleStepClimbing()
    {
        if (isPushing || moveAmount == 0f || Time.fixedTime - lastStepTime < 0.1f) 
            return;

        Vector3 lowerRayStart = transform.position + Vector3.up * 0.1f;
        Vector3 upperRayStart = transform.position + Vector3.up * stepHeight;
        Vector3 forwardDir = moveDir.normalized;

        // Se in modalità stealth, usa una forza dello step maggiorata
        float stepForceToUse = IsStealth ? stepForce * 1.3f : stepForce;

        // prova tre direzioni per scalare
        if (TryStepUp(lowerRayStart, upperRayStart, forwardDir, stepForceToUse)) return;
        if (TryStepUp(lowerRayStart, upperRayStart, Quaternion.Euler(0, 45, 0) * forwardDir, stepForceToUse * 0.7f)) return;
        if (TryStepUp(lowerRayStart, upperRayStart, Quaternion.Euler(0, -45, 0) * forwardDir, stepForceToUse * 0.7f)) return;
    }

    private bool TryStepUp(Vector3 lowerStart, Vector3 upperStart, Vector3 dir, float force)
    {
        // Raycast basso per rilevare la presenza di un ostacolo entro stepDistance
        if (!Physics.Raycast(lowerStart, dir, out var lowerHit, stepDistance, groundLayer))
            return false;
        
        if (lowerHit.collider.CompareTag("Pushable") || lowerHit.collider.transform.root.CompareTag("Pushable"))
            return false;

        // Calcola l'altezza del gradino rispetto alla posizione del personaggio 
        float heightDiff = lowerHit.point.y - transform.position.y;
        if (heightDiff <= 0.01f) return false;
        

        if (Physics.Raycast(upperStart, dir, stepDistance, groundLayer))
            return false;

        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        lastStepTime = Time.fixedTime;
        return true;
    }

    private void HandlePush()
    {
        bool isPressing = Input.GetMouseButton(0);
        bool canPush = currentPushableObject != null;
        
        if (isPressing && canPush)
        {
            // Verifica che il giocatore stia guardando nella direzione giusta
            Vector3 forward = transform.forward;
            Vector3 toObject = (currentPushableObject.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(forward, toObject); 
            
            if (dot > 0.5f) // Soglia per iniziare la spinta
            {
                StartPushing();
            }
            else
            {
                StopPushing();
            }
        }
        else
        {
            StopPushing();
        }
    }

    private void StartPushing()
    {
        if (!isPushing)
        {
            isPushing = true;
            prevStealth = IsStealth;
            animator.SetBool("IsPushing", true);
            animator.SetBool("IsStealth", false);
        }
        
        // Applica forza all'oggetto
        Rigidbody pushableRb = currentPushableObject.GetComponent<Rigidbody>();
        if (pushableRb != null)
        {
            Vector3 pushDir = transform.forward;
            pushDir.y = 0f;
            pushDir.Normalize();   
            pushableRb.AddForce(pushDir * pushForce, ForceMode.Force);
        }
        
        // Aggiorna le animazioni
        animator.SetBool("IsPushing", true);
        animator.SetBool("IsStealth", false);
    }

    private void StopPushing()
    {
        if (!isPushing) return;

        isPushing = false;
        currentVelocity = Vector3.zero;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        
        animator.SetBool("IsPushing", false);
        IsStealth = prevStealth;
        animator.SetBool("IsStealth", IsStealth);
    }

    #region Collision Detection
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pushable"))
        {
            Vector3 forward = transform.forward;
            Vector3 toObject = (collision.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(forward, toObject);

            // Se il giocatore sta guardando l'oggetto, riduci la velocità per evitare sovrapposizioni
            if (dot > 0.8f)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            
            currentPushableObject = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pushable") && collision.gameObject == currentPushableObject)
        {
            currentPushableObject = null;
            StopPushing();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hidable"))
        {
            isHidable = true;
            if (hidePrompt != null)
                hidePrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Hidable"))
        {
            isHidable = false;
            if (hidePrompt != null)
                hidePrompt.SetActive(false);
        }
    }
    #endregion

    #region Public Methods
    public void KillMe()
    {
        if (IsDead) return;

        IsDead = true;
        CanMove = false;
        animator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime);
        animator.SetTrigger("Die");
        
        StartCoroutine(WaitForDeathAnimation());
    }

    private System.Collections.IEnumerator WaitForDeathAnimation()
{
    
    yield return new WaitForSeconds(3f);
    
   
    GameManager.Instance.OnPlayerDeath();
}

    public bool IsPushingObject()
    {
        return isPushing;
    }

    public GameObject GetCurrentPushableObject()
    {
        return currentPushableObject;
    }
    #endregion

    #region Audio
    private void PlayHideToggleSound()
    {
        if (audioSource != null && hideToggleSound != null)
        {
            audioSource.PlayOneShot(hideToggleSound);
        }
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 0.1f);
        
        if (isPushing && currentPushableObject != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);
        }
    }
    #endregion
}