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

    [Header("Step Climbing Settings")]
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepDistance = 0.5f;
    [SerializeField] float stepForce = 2f;

    public bool IsStealth = false;
    public static bool IsHidden = false;

    public bool IsMoving = false;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Animator animator;
    private CameraController cameraController;

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

        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
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
        if (!IsHidden)
        {
            GroundCheck();
            HandleMovement();
            HandleStepClimbing();
        }

    }

    private void CacheInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        if (input.magnitude > 1f) input = input.normalized;

        moveDir = cameraController.PlanarRotation * input;
        moveAmount = input.magnitude;
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
            rb.useGravity = !IsHidden;
            capsuleCollider.enabled = !IsHidden;
            graphics.SetActive(!IsHidden);

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
        Vector3 targetVelocity = moveDir * speed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void HandleRotation()
    {
        if (moveAmount <= 0.01f) return;
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

    void HandleStepClimbing()
    {
        if (moveAmount == 0f || Time.fixedTime - lastStepTime < 0.1f) return;

        Vector3 lowerRayStart = transform.position + Vector3.up * 0.1f;
        Vector3 upperRayStart = transform.position + Vector3.up * stepHeight;
        Vector3 forwardDir = moveDir.normalized;

        RaycastHit hitLower;
        bool lowerHit = Physics.Raycast(lowerRayStart, forwardDir, out hitLower, stepDistance, groundLayer);

        if (lowerHit)
        {
            RaycastHit hitUpper;
            bool upperHit = Physics.Raycast(upperRayStart, forwardDir, out hitUpper, stepDistance, groundLayer);

            if (!upperHit)
            {
                float currentStepForce = IsStealth ? stepForce * 1.3f : stepForce;
                rb.AddForce(Vector3.up * currentStepForce, ForceMode.VelocityChange);
                lastStepTime = Time.fixedTime;
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
                float diagonalForce = IsStealth ? stepForce * 0.7f : stepForce * 0.4f;
                rb.AddForce(Vector3.up * diagonalForce, ForceMode.VelocityChange);
                lastStepTime = Time.fixedTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Hidable")
        {
            isHidable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hidable")
        {
            isHidable = false;
        }
    }

}