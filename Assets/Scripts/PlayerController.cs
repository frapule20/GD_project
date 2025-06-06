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
    public bool IsDead = false; 

    public bool CanMove = true;

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
        Debug.Log("Player cannot move at the moment: " + CanMove);
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
        float heightDiff = lowerHit.point.y - transform.position.y;
        if (heightDiff <= 0.01f) return false;
        if (Physics.Raycast(upperStart, dir, stepDistance, groundLayer)) return false;

        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        lastStepTime = Time.fixedTime;
        return true;
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