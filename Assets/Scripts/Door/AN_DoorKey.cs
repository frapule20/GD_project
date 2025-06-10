using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_DoorKey : MonoBehaviour
{
    [Tooltip("True - red key object, false - blue key")]
    public bool isRedKey = true;
    PlayerController player;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;

    public GameObject grabPrompt;
    private bool wasNear = false;
    private bool isPickedUp = false;

    [Header("Audio")]
    [Tooltip("Suono da riprodurre quando il giocatore prede le chiavi")]
    public AudioClip pickupSound;

    private AudioSource audioSource;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>(); // key will get up and it will saved in "inventary"
        audioSource = GetComponent<AudioSource>();
        if (grabPrompt != null)
        {
            grabPrompt.SetActive(true);
            grabPrompt.SetActive(false);
        } 
    }

    void Update()
    {
        if (isPickedUp) return;
        bool isNear = NearView();

        if (isNear && !wasNear)
        {
            // Si è avvicinato
            if (grabPrompt) grabPrompt.SetActive(true);
            wasNear = true;
        }
        else if (!isNear && wasNear)
        {
            // Si è allontanato senza prendere l'oggetto
            if (grabPrompt) grabPrompt.SetActive(false);
            wasNear = false;
        }

        if (NearView() && Input.GetKeyDown(KeyCode.Space))
        {
             PickupKey();
        }
    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
        direction = transform.position - player.transform.position;
        angleView = Vector3.Angle(player.transform.forward, direction);
        return distance < 2f;
    }

    private void PickupKey()
    {
        if (isPickedUp) return; // Evita doppi pickup
        
        isPickedUp = true;
        if (grabPrompt) grabPrompt.SetActive(false);
        
        if (isRedKey) player.RedKey = true;
        else player.BlueKey = true;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;
            
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;
        
        StartCoroutine(PlaySoundAndDestroy());
    }
    
    private IEnumerator PlaySoundAndDestroy()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);
        }
        
        Destroy(gameObject);
    }
}
