using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector2 framingOffset;

    [Header("Camera Movement")]
    [SerializeField] private float rotationSpeed = 1.5f;
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 45f;
    [SerializeField] private float smoothTime = 0.03f;

    [Header("Input Settings")]
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    [Header("Obstruction Handling")]
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float clipPadding = 0.1f;

    [Header("Top-Down Mode")]
    [SerializeField] private Vector3 topDownOffset = new Vector3(0, 5, 0);
    [SerializeField] private float transitionSpeed = 4f;
    [SerializeField] private float transitionThreshold = 0.1f;
    [SerializeField] private float rotationThreshold = 1f;

    // Rotation values
    private float rotationX;
    private float rotationY;

    // Cached values to avoid recalculation
    private float invertXVal;
    private float invertYVal;
    private Vector3 focusPosition;
    private Vector3 desiredPosition;
    private Quaternion targetRotation;

    // Top-down mode state
    private bool isInTopDownMode;
    private bool wasInTopDown;
    
    // Smooth movement
    private Vector3 cameraVelocity;

    // Cached components and calculations
    private Transform cameraTransform;
    private Vector3 forwardDirection;
    
    // Raycast optimization
    private RaycastHit obstructionHit;
    private readonly RaycastHit[] raycastResults = new RaycastHit[1];

    #region Unity Lifecycle

    private void Awake()
    {
        // Cache transform reference
        cameraTransform = transform;
    }

    private void Start()
    {
        InitializeCamera();
        CacheInvertValues();
    }

    private void LateUpdate()
    {
        UpdateCameraMode();
        
        if (isInTopDownMode)
        {
            HandleTopDownMode();
        }
        else
        {
            HandleNormalMode();
        }
    }

    #endregion

    #region Initialization

    private void InitializeCamera()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rotationY = followTarget.eulerAngles.y;
    }

    private void CacheInvertValues()
    {
        invertXVal = invertX ? -1f : 1f;
        invertYVal = invertY ? -1f : 1f;
    }

    #endregion

    #region Camera Mode Management

    private void UpdateCameraMode()
    {
        bool shouldBeInTopDown = PlayerController.IsHidden;
        
        if (shouldBeInTopDown != isInTopDownMode)
        {
            isInTopDownMode = shouldBeInTopDown;
            if (isInTopDownMode)
            {
                wasInTopDown = true;
            }
        }
    }

    #endregion

    #region Normal Camera Mode

    private void HandleNormalMode()
    {
        ProcessMouseInput();
        CalculateTargetTransform();
        HandleObstruction();
        ApplyCameraTransform();
    }

    private void ProcessMouseInput()
    {
        // Considera di usare Input System invece di Input legacy
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationX += mouseY * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += mouseX * invertXVal * rotationSpeed;
    }

    private void CalculateTargetTransform()
    {
        targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        // Cache focus position calculation
        focusPosition.x = followTarget.position.x + framingOffset.x;
        focusPosition.y = followTarget.position.y + framingOffset.y;
        focusPosition.z = followTarget.position.z;

        // Cache forward direction
        forwardDirection = targetRotation * Vector3.forward;
        
        // Calculate desired position
        desiredPosition = focusPosition - forwardDirection * distance;
    }

    private void HandleObstruction()
    {
        // Ottimizzazione: usa Physics.RaycastNonAlloc per evitare allocazioni
        int hitCount = Physics.RaycastNonAlloc(focusPosition, 
            (desiredPosition - focusPosition).normalized, 
            raycastResults, 
            Vector3.Distance(focusPosition, desiredPosition), 
            obstructionMask);

        if (hitCount > 0)
        {
            obstructionHit = raycastResults[0];
            desiredPosition = obstructionHit.point + obstructionHit.normal * clipPadding;
        }
    }

    private void ApplyCameraTransform()
    {
        if (wasInTopDown)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, 
                Time.deltaTime * transitionSpeed);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, 
                Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(cameraTransform.position, desiredPosition) < transitionThreshold &&
                Quaternion.Angle(cameraTransform.rotation, targetRotation) < rotationThreshold)
            {
                wasInTopDown = false;
            }
        }
        else
        {
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, 
                ref cameraVelocity, smoothTime);
            cameraTransform.rotation = targetRotation;
        }
    }

    #endregion

    #region Top-Down Mode

    private void HandleTopDownMode()
    {
        Vector3 targetPosition = followTarget.position + topDownOffset;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down);

        float deltaTime = Time.deltaTime;
        float lerpSpeed = deltaTime * transitionSpeed;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, lerpSpeed);
        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, lerpSpeed);
    }

    #endregion

    #region Public Interface

    public bool IsInTopDownMode() => isInTopDownMode;
    
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

    public void SetInvertSettings(bool invertXAxis, bool invertYAxis)
    {
        invertX = invertXAxis;
        invertY = invertYAxis;
        CacheInvertValues();
    }

    public void SetFollowTarget(Transform newTarget)
    {
        followTarget = newTarget;
        if (followTarget != null)
        {
            rotationY = followTarget.eulerAngles.y;
        }
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Assicurati che i valori siano validi nell'editor
        minVerticalAngle = Mathf.Clamp(minVerticalAngle, -90f, 0f);
        maxVerticalAngle = Mathf.Clamp(maxVerticalAngle, 0f, 90f);
        distance = Mathf.Max(0.1f, distance);
        transitionSpeed = Mathf.Max(0.1f, transitionSpeed);
        smoothTime = Mathf.Max(0.001f, smoothTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (followTarget == null) return;

        // Visualizza la posizione focus
        Gizmos.color = Color.yellow;
        Vector3 focus = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);
        Gizmos.DrawWireSphere(focus, 0.1f);

        // Visualizza la distanza desiderata
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(focus, desiredPosition);
        }
        else
        {
            Vector3 previewPos = focus - transform.forward * distance;
            Gizmos.DrawLine(focus, previewPos);
        }

        // Visualizza la posizione top-down
        Gizmos.color = Color.green;
        Vector3 topDownPos = followTarget.position + topDownOffset;
        Gizmos.DrawWireCube(topDownPos, Vector3.one * 0.5f);
    }
    #endif

    #endregion
}