using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;

    [SerializeField] float rotationSpeed = 1.5f;
    [SerializeField] float distance = 4f;
    [SerializeField] float minVerticalAngle = -10;
    [SerializeField] float maxVerticalAngle = 45;
    

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    [SerializeField] LayerMask obstructionMask;
    [SerializeField] float clipPadding = 0.1f;
    [SerializeField] float smoothTime = 0.03f;


    [Header("Top-Down Mode")]
    [SerializeField] Vector3 topDownOffset = new Vector3(0, 5, 0);
    [SerializeField] float transitionSpeed = 4f;

    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    
    private Vector3 normalPosition;
    private Quaternion normalRotation;
    private bool wasInTopDown = false;
    private Vector3 cameraVelocity;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        rotationY = followTarget.eulerAngles.y;
    }
    private void LateUpdate()
    {
        if (PlayerController.IsHidden)
        {
            TopDownMode();
            return;
        }

        invertXVal = invertX ? -1f : 1f;
        invertYVal = invertY ? -1f : 1f;

        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 focusPos = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);
        Vector3 desiredPos = focusPos - targetRotation * Vector3.forward * distance;

        Vector3 clippedPos = desiredPos;

        if (Physics.Linecast(focusPos, desiredPos, out RaycastHit hit, obstructionMask))
            clippedPos = hit.point + hit.normal * clipPadding;

        if (wasInTopDown)
        {
            transform.position = Vector3.Lerp(transform.position, clippedPos, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(transform.position, clippedPos) < 0.1f &&
                Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                wasInTopDown = false;
            }
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, clippedPos, ref cameraVelocity, smoothTime);
            transform.rotation = targetRotation;
        }
    }

    private void TopDownMode()
    {
        if (!wasInTopDown)
        {
            wasInTopDown = true;
        }

        Vector3 targetPosition = followTarget.position + topDownOffset;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
    }

    public bool IsInTopDownMode()
    {
        return PlayerController.IsHidden;
    }
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}