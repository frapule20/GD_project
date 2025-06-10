using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Tooltip("Oggetto che compare quando il puzzle è risolto")]
    public GameObject rewardObject;

    [Tooltip("Stato iniziale della chaive")]
    public bool startActive = false;

    [Header("Audio")]
    [Tooltip("AudioSource per riprodurre i suoni")]
    public AudioSource audioSource;
    
    [Tooltip("Suono da riprodurre quando il puzzle è completato")]
    public AudioClip puzzleCompletedSound;

    private bool[] boxesOnTrigger = new bool[2] { false, false };
    private bool puzzleWasCompleted = false;

    private void Start()
    {
        Debug.Log("PuzzleManager started");
        if (rewardObject != null)
            rewardObject.SetActive(startActive);
            puzzleWasCompleted = startActive;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }


    public void SetBoxOnTrigger(int index, bool isOn)
    {
        Debug.Log($"SetBoxOnTrigger called with index: {index}, isOn: {isOn}");
        if (index < 0 || index >= boxesOnTrigger.Length) return;

        boxesOnTrigger[index] = isOn;
        CheckPuzzleComplete();
    }

    // Controlla se entrambe le casse sono posizionate
    private void CheckPuzzleComplete()
    {
        Debug.Log($"Checking puzzle complete: {boxesOnTrigger[0]}, {boxesOnTrigger[1]}");
        bool allOn = boxesOnTrigger[0] && boxesOnTrigger[1];
        
        if (rewardObject != null)
        {
            if (allOn && !puzzleWasCompleted)
            {
                rewardObject.SetActive(true);
                PlayPuzzleCompletedSound();
                puzzleWasCompleted = true;
            }
            else if (!allOn && puzzleWasCompleted)
            {
                rewardObject.SetActive(false);
                puzzleWasCompleted = false;
            }
        }
    }

    private void PlayPuzzleCompletedSound()
    {
        if (audioSource != null && puzzleCompletedSound != null)
        {
            audioSource.PlayOneShot(puzzleCompletedSound);
            Debug.Log("Puzzle completed sound played");
        }
        else
        {
            Debug.LogWarning("AudioSource o AudioClip non assegnati per il suono del puzzle completato");
        }
    }
}
