using UnityEngine;
using System.Collections;

public class HalfCheck : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip audioClip;
    public GameObject dialogue;
    public float volume = 1.0f;

    private AudioSource audioSource;

    private bool hasTriggered = false;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        if (dialogue != null) dialogue.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra è il player e se l'audio non è già stato riprodotto
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Riproduci l'audio
            audioSource.Play();
            if (dialogue != null)
            {
                dialogue.SetActive(true);
                StartCoroutine(HideDialogueAfterDelay(6f));
            }

            // Segna che l'audio è stato riprodotto
            hasTriggered = true;

            Debug.Log("Audio riprodotto!");
        }
    }
    
        private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dialogue != null)
            dialogue.SetActive(false);
    }
}