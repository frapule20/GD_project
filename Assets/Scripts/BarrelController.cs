using UnityEngine;

public class BarrelController : MonoBehaviour
{
    Rigidbody rb;
    GameObject playerNearby;
    bool canBePushed;
    bool isBeingPushed;
    AudioSource audioSource;

    [Header("Drag Settings")]
    [SerializeField] float pushDrag = 8f;
    [SerializeField] float pushAngularDrag = 15f;
    [SerializeField] float stopThreshold = 0.1f; // Soglia per fermare completamente l'oggetto

    [Header("Audio")]
    [SerializeField] AudioClip pushLoopClip;
    
    float defaultDrag;
    float defaultAngularDrag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;

        // Inizialmente l'oggetto è completamente bloccato
        rb.constraints = RigidbodyConstraints.FreezeAll;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = pushLoopClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool playerPressingPush = (playerNearby != null) && Input.GetMouseButton(0);
        
        // Controlla se il giocatore sta guardando nella direzione dell'oggetto
        bool playerLookingAtBarrel = false;
        if (playerNearby != null)
        {
            Vector3 playerForward = playerNearby.transform.forward;
            Vector3 toBarrel = (transform.position - playerNearby.transform.position).normalized;
            float dot = Vector3.Dot(playerForward, toBarrel);
            playerLookingAtBarrel = dot > 0.5f;
        }

        bool shouldBePushable = playerPressingPush && playerLookingAtBarrel;

        // Attiva la modalità push
        if (shouldBePushable && !canBePushed)
        {
            StartPushMode();
        }
        // Disattiva la modalità push
        else if (!shouldBePushable && canBePushed)
        {
            StopPushMode();
        }
        
        // Se è in modalità push, controlla se si sta ancora muovendo
        if (canBePushed)
        {
            // Se la velocità è molto bassa, ferma completamente l'oggetto
            if (rb.linearVelocity.magnitude < stopThreshold)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void StartPushMode()
    {
        canBePushed = true;
        isBeingPushed = true;
        
        // Sblocca solo gli assi necessari per il movimento
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        
        // Applica il drag per un movimento più controllato
        rb.linearDamping = pushDrag;
        rb.angularDamping = pushAngularDrag;

        if (pushLoopClip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    void StopPushMode()
    {
        canBePushed = false;
        isBeingPushed = false;

        audioSource.Stop();
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.linearDamping = defaultDrag;
        rb.angularDamping = defaultAngularDrag;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = collision.gameObject;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == playerNearby)
        {
            playerNearby = null;
            
            // Se il giocatore si allontana, ferma la modalità push
            if (canBePushed)
            {
                StopPushMode();
            }
        }
    }

    // Metodo pubblico per verificare se l'oggetto può essere spinto (utile per debug)
    public bool CanBePushed()
    {
        return canBePushed;
    }
    
    public bool IsBeingPushed()
    {
        return isBeingPushed;
    }
}