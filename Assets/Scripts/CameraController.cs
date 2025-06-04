using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;

    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float distance = 5;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = 45;

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;


    [Header("Top-Down Mode")]
    [SerializeField] Vector3 topDownOffset = new Vector3(0, 5, 0);
    [SerializeField] float transitionSpeed = 3f;

    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    
    private Vector3 normalPosition;
    private Quaternion normalRotation;
    private bool wasInTopDown = false;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (PlayerController.IsHidden)
        {
            TopDownMode();
        }
        else
        {
            NormalMode();
        }
    }
   private void NormalMode()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        Vector3 targetPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        if (wasInTopDown)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f && 
                Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                wasInTopDown = false;
            }
        }
        else
        {
            transform.position = targetPosition;
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