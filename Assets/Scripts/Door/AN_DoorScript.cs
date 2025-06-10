using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_DoorScript : MonoBehaviour
{
    [Tooltip("If it is false door can't be used")]
    public bool Locked = false;
    [Tooltip("It is true for remote control only")]
    public bool Remote = false;
    [Space]
    [Tooltip("Door can be opened")]
    public bool CanOpen = true;
    [Tooltip("Door can be closed")]
    public bool CanClose = true;
    [Space]
    [Tooltip("Door locked by red key (use key script to declarate any object as key)")]
    public bool RedLocked = false;
    public bool BlueLocked = false;
    [Tooltip("It is used for key script working")]
    PlayerController player;
    [Space]

    [Header("UI Prompts")]
    [Tooltip("Prompt shown when player can open the door")]
    public GameObject OpenDoorPrompt;
    [Tooltip("Prompt shown when player is missing a key")]
    public GameObject MissingPrompt;
    [Space]
    public bool isOpened = false;
    [Range(0f, 4f)]
    [Tooltip("Speed for door opening, degrees per sec")]
    public float OpenSpeed = 3f;

    [Header("Audio")]
    [Tooltip("Suono da riprodurre quando il giocatore prede le chiavi")]
    public AudioClip doorOpenSound;

    private AudioSource audioSource;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;
    

    // Hinge
    [HideInInspector]
    public Rigidbody rbDoor;
    HingeJoint hinge;
    JointLimits hingeLim;
    float currentLim;
    
    // Stato stabile della porta
    private bool doorStateChanged = false;
    private float stateChangeTimer = 0f;
    private const float STATE_CHANGE_COOLDOWN = 1f;

    void Start()
    {
        rbDoor = GetComponent<Rigidbody>();
        rbDoor.mass = 10f;
        hinge = GetComponent<HingeJoint>();
        player = FindFirstObjectByType<PlayerController>();
        audioSource = GetComponent<AudioSource>();

        // Nascondi i prompts
        if (OpenDoorPrompt) OpenDoorPrompt.SetActive(false);
        if (MissingPrompt) MissingPrompt.SetActive(false);
    }

    void Update()
    {
        if (doorStateChanged)
        {
            stateChangeTimer += Time.deltaTime;
            if (stateChangeTimer >= STATE_CHANGE_COOLDOWN)
            {
                doorStateChanged = false;
                stateChangeTimer = 0f;
            }
        }


        bool nearDoor = NearView();
        
        if (nearDoor)
        {
            ShowPrompts();
        }
        else
        {
            HidePrompts();
        }
        

        if (!Remote && Input.GetKeyDown(KeyCode.Space) && NearView() && !doorStateChanged)
        {
            Action();
        }
    }

    void ShowPrompts()
    {
        if (isOpened)
        {           
            HidePrompts();
            return;
        }
        // Se può aprire/chiudere la porta
        if (CanOpenDoor())
        {
            if (OpenDoorPrompt) OpenDoorPrompt.SetActive(true);
            if (MissingPrompt) MissingPrompt.SetActive(false);
        }
        // Se manca qualcosa
        else
        {
            if (OpenDoorPrompt) OpenDoorPrompt.SetActive(false);
            if (MissingPrompt) MissingPrompt.SetActive(true);
        }
    }
    
    void HidePrompts()
    {
        if (OpenDoorPrompt) OpenDoorPrompt.SetActive(false);
        if (MissingPrompt) MissingPrompt.SetActive(false);
    }

    bool CanOpenDoor()
    {
        // Se è già aperta può chiudere
        if (isOpened) return true;
        
        // Controlla se ha le chiavi necessarie
        bool hasRed = !RedLocked || (player && player.RedKey);
        bool hasBlue = !BlueLocked || (player && player.BlueKey);
        
        return hasRed && hasBlue;
    }

    public void Action() // void to open/close door
    {

        if (!Locked && !doorStateChanged)
        {
            // key lock checking
            if (player != null && RedLocked && player.RedKey)
            {
                RedLocked = false;
                player.RedKey = false;
            }
            else if (player != null && BlueLocked && player.BlueKey)
            {
                BlueLocked = false;
                player.BlueKey = false;
            }

            // opening/closing
            if (isOpened && CanClose && !RedLocked && !BlueLocked)
            {
                rbDoor.mass = 10f;
                isOpened = false;
                doorStateChanged = true;
                PlayDoorSound();
            }
            else if (!isOpened && CanOpen && !RedLocked && !BlueLocked)
            {
                // APERTURA PORTA: massa = 1
                rbDoor.mass = 1f;
                isOpened = true;
                doorStateChanged = true; // Blocca cambi di stato per un po'
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
                PlayDoorSound();
            }
        }
        else if (doorStateChanged)
        {
            Debug.Log("DOOR ACTION BLOCKED - State change cooldown active");
        }
        else
        {
            Debug.Log("DOOR ACTION BLOCKED - Door is locked!");
        }

    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
        direction = transform.position - player.transform.position;
        angleView = Vector3.Angle(player.transform.forward, direction);
        
        bool nearEnough = distance < 3f; // angleView < 35f && 
        
        return nearEnough;
    }

    private void FixedUpdate() // door is physical object
    {
        // Aggiorna i limiti del hinge in base allo stato della porta
        if (isOpened)
        {
            currentLim = 85f;
        }
        else
        {
            // Chiudi gradualmente la porta
            if (currentLim > 1f)
                currentLim -= 0.5f * OpenSpeed;
        }

        // Applica i limiti al hinge
        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
    }

    private void PlayDoorSound()
    {
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
    }
}