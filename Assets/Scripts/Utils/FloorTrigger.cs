using UnityEngine;

public class FloorTrigger : MonoBehaviour
{
    [Tooltip("Tag che identifica le casse")]
    public string boxTag = "Pushable";

    [Tooltip("Riferimento al PuzzleManager")]
    public PuzzleManager manager;

    [Tooltip("Indice di questo trigger (0 o 1)")]
    public int triggerIndex;

    [Header("Audio")]
    public AudioClip pressureSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(boxTag))
        {
            manager.SetBoxOnTrigger(triggerIndex, true);
            PlayPressureSound();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Trigger {triggerIndex} hit by: {other.name} with tag: {other.tag}");
        if (other.CompareTag(boxTag))
        {
            Debug.Log($"Trigger {triggerIndex} exited by: {other.name}");
            manager.SetBoxOnTrigger(triggerIndex, false);
            PlayPressureSound();
        }
    }
    
    private void PlayPressureSound()
    {
        if (audioSource != null && pressureSound != null)
        {
            audioSource.PlayOneShot(pressureSound);
        }
    }
}
