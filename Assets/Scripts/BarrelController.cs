using UnityEngine;

public class BarrelController : MonoBehaviour
{
    Rigidbody rb;
    GameObject playerNearby;
    bool canBePushed;

    [Header("Drag Settings")]
    [SerializeField] float pushDrag = 8f;
    [SerializeField] float pushAngularDrag = 15f;
    float defaultDrag;
    float defaultAngularDrag;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    void Update()
    {
        bool pressing = (playerNearby != null) && Input.GetMouseButton(0);

        if (pressing && !canBePushed)
        {
            canBePushed = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation
               | RigidbodyConstraints.FreezePositionY;
            rb.linearDamping = pushDrag;
            rb.angularDamping  = pushAngularDrag;
        }
        else if (!pressing && canBePushed)
        {
            canBePushed = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearDamping= defaultDrag;
            rb.angularDamping = defaultAngularDrag;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
            playerNearby = c.gameObject;
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject == playerNearby)
            playerNearby = null;
    }
}
