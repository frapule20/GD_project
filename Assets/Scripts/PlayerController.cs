using NUnit.Framework;
using UnityEditor;
using UnityEngine;



[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float rotationSpeed = 600f;
    [SerializeField] float acceleration = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;


    [Header("Push Settings")]
    [SerializeField] float forceapll = 10f;
    private Rigidbody currentPushRb;
    private float originalDrag, originalAngularDrag;

    private enum PushAxis { None, X, Z }
    private PushAxis currentPushAxis = PushAxis.None;
    private RigidbodyConstraints originalConstraints;


    [Header("Step Climbing Settings")]
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepDistance = 0.5f;
    [SerializeField] float stepForce = 2f;

    [Header("Audio")]
    [Tooltip("Suono da riprodurre quando il giocatore si nasconde/mostra")]
    public AudioClip hideToggleSound;
    
    private AudioSource audioSource;


    public bool IsStealth = false;
    public static bool IsHidden = false;
    public bool IsMoving = false;
    public bool IsDead = false;
    public bool CanMove = true;

    public bool RedKey = false;
    public bool BlueKey = false;

    [Header("UI Elements")]
    public GameObject hidePrompt;


    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Animator animator;
    private CameraController cameraController;

    private FixedJoint pushJoint;
    private Rigidbody pushedRigidbody;

    GameObject graphics;

    private bool isGrounded;
    private bool isHidable = false;


    private Quaternion targetRotation;
    private Vector3 currentVelocity;
    private float h, v;
    private Vector3 moveDir;
    private float moveAmount;
    private float lastStepTime;

    private void Awake()
    {
        graphics = transform.GetChild(0).gameObject;
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<CameraController>();
        audioSource = GetComponent<AudioSource>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        originalConstraints = rb.constraints;
        if (hidePrompt != null) hidePrompt.SetActive(false);

    }

    private void Update()
    {
        if (IsDead || !CanMove)
        {
            animator.SetFloat("moveAmount", 0f, 0f, Time.deltaTime);
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            currentVelocity = Vector3.zero;
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

    private void CacheInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        if (input.magnitude > 1f) input = input.normalized;

        moveDir = cameraController.PlanarRotation * input;
        moveAmount = input.magnitude;

        if (currentPushRb != null)
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
        IsMoving = moveAmount != 0f;
    }

    private void HandleMovement()
    {
        float speed = IsStealth ? walkSpeed : runSpeed;
        Vector3 targetVelocity;

        if (currentPushRb != null)
            targetVelocity = moveDir * walkSpeed;
        else
            targetVelocity = moveDir * speed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void HandleRotation()
    {
        if (moveAmount <= 0.01f) return;
        if (currentPushRb != null) return;
        targetRotation = Quaternion.LookRotation(moveDir);
        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        ));
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
    }

    public void KillMe()
    {
        if (IsDead) return;
        IsDead = true;
        CanMove = false;
        animator.SetFloat("moveAmount", moveAmount, 0.1f, Time.deltaTime);
        animator.SetTrigger("Die");
    }

    void HandleStepClimbing()
    {
        if (currentPushRb != null) return;
        if (moveAmount == 0f || Time.fixedTime - lastStepTime < 0.1f) return;

        Vector3 lowerRayStart = transform.position + Vector3.up * 0.1f;
        Vector3 upperRayStart = transform.position + Vector3.up * stepHeight;
        Vector3 forwardDir = moveDir.normalized;

        if (TryStepUp(lowerRayStart, upperRayStart, forwardDir, IsStealth ? stepForce * 1.3f : stepForce)) return;
        if (TryStepUp(lowerRayStart, upperRayStart, Quaternion.Euler(0, 45, 0) * forwardDir, IsStealth ? stepForce * 0.7f : stepForce * 0.4f)) return;
        if (TryStepUp(lowerRayStart, upperRayStart, Quaternion.Euler(0, -45, 0) * forwardDir, IsStealth ? stepForce * 0.7f : stepForce * 0.4f)) return;
    }

    bool TryStepUp(Vector3 lowerStart, Vector3 upperStart, Vector3 dir, float force)
    {
        if (!Physics.Raycast(lowerStart, dir, out var lowerHit, stepDistance, groundLayer)) return false;
        if (lowerHit.collider.CompareTag("Pushable") || lowerHit.collider.transform.root.CompareTag("Pushable"))
            return false;
        float heightDiff = lowerHit.point.y - transform.position.y;
        if (heightDiff <= 0.01f) return false;
        if (Physics.Raycast(upperStart, dir, stepDistance, groundLayer)) return false;

        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        lastStepTime = Time.fixedTime;
        return true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hidable")
        {
            isHidable = true;
            hidePrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hidable")
        {
            isHidable = false;
            hidePrompt.SetActive(false);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pushable"))
        {
            Debug.Log("Collision with Pushable detected");
            Vector3 forward = transform.forward;
            Vector3 toObject = (collision.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(forward, toObject);

            if (dot > 0.8f)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    void HandlePush()
    {
        bool isPressing = Input.GetMouseButton(0);

        if (isPressing)
        {
            if (currentPushRb == null)
            {
                Vector3 origin = transform.position + Vector3.up * 0.6f;
                if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 1.5f)
                    && hit.collider.CompareTag("Pushable"))
                {
                    currentPushRb = hit.collider.attachedRigidbody;
                    originalDrag = currentPushRb.linearDamping;
                    originalAngularDrag = currentPushRb.angularDamping;
                    currentPushRb.linearDamping = 8f;
                    currentPushRb.angularDamping = 8f;

                    Vector3 dir = transform.forward;
                    currentPushAxis = Mathf.Abs(dir.x) > Mathf.Abs(dir.z) ? PushAxis.X : PushAxis.Z;

                    RigidbodyConstraints freezePos = RigidbodyConstraints.FreezePositionY;
                    freezePos |= (currentPushAxis == PushAxis.X)
                                ? RigidbodyConstraints.FreezePositionZ
                                : RigidbodyConstraints.FreezePositionX;
                    rb.constraints = freezePos
                                | RigidbodyConstraints.FreezeRotation;
                }
            }

            if (currentPushRb != null)
            {
                Vector3 pushDir = transform.forward;
                pushDir.y = 0f;
                pushDir.Normalize();
                currentPushRb.AddForceAtPosition(pushDir * forceapll, currentPushRb.worldCenterOfMass, ForceMode.Force);
                animator.SetBool("IsPushing", true);
                animator.SetBool("IsStealth", false);
                return;
            }
        }

        if (currentPushRb != null)
        {
            currentPushRb.linearDamping = originalDrag;
            currentPushRb.angularDamping = originalAngularDrag;
            currentPushRb = null;
            currentPushAxis = PushAxis.None;
            rb.constraints = originalConstraints;

        }

        animator.SetBool("IsPushing", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 0.1f);
    }
    
    private void PlayHideToggleSound()
    {
        if (audioSource != null && hideToggleSound != null)
        {
            audioSource.PlayOneShot(hideToggleSound);
        }
    }

}