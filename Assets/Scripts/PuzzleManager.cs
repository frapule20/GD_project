using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Tooltip("Oggetto che compare quando il puzzle è risolto")]
    public GameObject rewardObject;

    [Tooltip("Stato iniziale della chaive")]
    public bool startActive = false;

    private bool[] boxesOnTrigger = new bool[2] { false, false };

    private void Start()
    {
        Debug.Log("PuzzleManager started");
        if (rewardObject != null)
            rewardObject.SetActive(startActive);
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
            rewardObject.SetActive(allOn);
    }
}
