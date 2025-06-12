using UnityEngine;
using System.Collections;

public class CheckpointTrigger : MonoBehaviour
{

    [Header("Checkpoint Type")]
    public CheckpointType type;

    [Header("Audio Settings")]
    public AudioClip audioClip;
    public GameObject dialogue;
    public float volume = 1.0f;

    private AudioSource audioSource;

    private bool hasTriggered = false;


    void Start()
    {
        if (audioClip != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioClip.LoadAudioData();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
        }

        if (dialogue != null)
        {
            dialogue.SetActive(true);
            dialogue.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {

            if (audioSource != null)
            {
            audioSource.Play();
            }
        
            if (dialogue != null)
            {
                dialogue.SetActive(true);
                StartCoroutine(HideDialogueAfterDelay(6f));
            }

            hasTriggered = true;

            switch (type)
            {
                case CheckpointType.Half:
                    GameManager.Instance.OnHalfCheckpointReached();
                    break;
                case CheckpointType.Final:
                    GameManager.Instance.OnPlayerWin();
                    break;
            }
        }
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dialogue != null)
            dialogue.SetActive(false);
    }

}

public enum CheckpointType
{
    Initial,
    Half,
    Final
}