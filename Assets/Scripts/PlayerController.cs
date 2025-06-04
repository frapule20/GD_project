using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float rotationSpeed = 500f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    [Header("Step Climbing Settings")]
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepDistance = 0.5f;
    [SerializeField] float stepSmooth = 2f;
    [SerializeField] float stepForce = 10f;

    private Rigidbody rb;
    private Animator animator;
    private CameraController cameraController;

    private bool isGrounded;
    private static bool IsStealth = false;
    private Quaternion targetRotation;
    private Vector3 lastMoveDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cameraController = Camera.main.GetComponent<CameraController>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        HandleStealthToggle();
        HandleRotation();
        animator.SetFloat("moveAmount", GetMoveAmount(), 0.2f, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
        HandleStepClimbing();
    }

    private void HandleStealthToggle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            IsStealth = !IsStealth;
            animator.SetBool("IsStealth", IsStealth);
        }
    }

    private float GetMoveAmount()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        return Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;
        Vector3 moveDir = cameraController.PlanarRotation * input;

        float speed = IsStealth ? walkSpeed : runSpeed;
        Vector3 newVelocity = moveDir * speed;
        newVelocity.y = rb.linearVelocity.y;

        lastMoveDir = moveDir;
        rb.linearVelocity = newVelocity;
    }

    private void HandleRotation()
    {
        float moveAmount = GetMoveAmount();
        if (moveAmount > 0f)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 input = new Vector3(h, 0, v).normalized;
            Vector3 moveDir = cameraController.PlanarRotation * input;
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(
            transform.TransformPoint(groundCheckOffset),
            groundCheckRadius
        );

        Gizmos.color = Color.red;
        Vector3 lowerPos = transform.position + Vector3.up * 0.1f;
        Vector3 upperPos = transform.position + Vector3.up * stepHeight;
        
        Gizmos.DrawRay(lowerPos, transform.forward * stepDistance);
        Gizmos.DrawRay(upperPos, transform.forward * stepDistance);
    }

    void HandleStepClimbing()
    {
        if (GetMoveAmount() == 0f) return;

        Vector3 lowerRayStart = transform.position + Vector3.up * 0.1f;
        Vector3 upperRayStart = transform.position + Vector3.up * stepHeight;
        Vector3 forwardDir = transform.forward;

        RaycastHit hitLower;
        bool lowerHit = Physics.Raycast(lowerRayStart, forwardDir, out hitLower, stepDistance, groundLayer);

        if (lowerHit)
        {
            RaycastHit hitUpper;
            bool upperHit = Physics.Raycast(upperRayStart, forwardDir, out hitUpper, stepDistance, groundLayer);

            if (!upperHit)
            {
                Vector3 stepUpForce = Vector3.up * stepForce;
                rb.AddForce(stepUpForce, ForceMode.Acceleration);
                
                Vector3 newPos = rb.position;
                newPos.y += stepSmooth * Time.fixedDeltaTime;
                rb.position = newPos;
            }
        }
        CheckStepInDirection(Quaternion.Euler(0, 45, 0) * forwardDir);
        CheckStepInDirection(Quaternion.Euler(0, -45, 0) * forwardDir);
    }

    private void CheckStepInDirection(Vector3 direction)
    {
        Vector3 lowerRayStart = transform.position + Vector3.up * 0.1f;
        Vector3 upperRayStart = transform.position + Vector3.up * stepHeight;

        RaycastHit hitLower;
        bool lowerHit = Physics.Raycast(lowerRayStart, direction, out hitLower, stepDistance * 0.7f, groundLayer);

        if (lowerHit)
        {
            RaycastHit hitUpper;
            bool upperHit = Physics.Raycast(upperRayStart, direction, out hitUpper, stepDistance * 0.7f, groundLayer);

            if (!upperHit)
            {
                Vector3 stepUpForce = Vector3.up * (stepForce * 0.5f);
                rb.AddForce(stepUpForce, ForceMode.Acceleration);
            }
        }
    }
}