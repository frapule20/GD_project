using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HintPromptTrigger : MonoBehaviour
{
    public GameObject dialogue;
    private bool hasTriggered = false;
    
    private static List<GameObject> activeDialogues = new List<GameObject>();
    
    private Coroutine hideDialogueCoroutine;

    void Start()
    {
        if (dialogue != null)
        {
            dialogue.SetActive(true);
            dialogue.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pushable"))
        {
            float originalY = other.transform.position.y;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            Vector3 newPosition = other.transform.position + pushDirection * 0.5f;
            newPosition.y = originalY;
            other.transform.position = newPosition;
            
            return; 
        }

        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (dialogue != null)
            {

                HideAllActiveDialogues();
                
                dialogue.SetActive(true);
                activeDialogues.Add(dialogue);

                hideDialogueCoroutine = StartCoroutine(HideDialogueAfterDelay(6f));
            }

            hasTriggered = true;
        }
    }


    private static void HideAllActiveDialogues()
    {
        foreach (GameObject activeDialogue in activeDialogues)
        {
            if (activeDialogue != null)
            {
                activeDialogue.SetActive(false);
            }
        }
        activeDialogues.Clear();
        
        HintPromptTrigger[] allTriggers = FindObjectsByType<HintPromptTrigger>(FindObjectsSortMode.None);
        foreach (HintPromptTrigger trigger in allTriggers)
        {
            if (trigger.hideDialogueCoroutine != null)
            {
                trigger.StopCoroutine(trigger.hideDialogueCoroutine);
                trigger.hideDialogueCoroutine = null;
            }
        }
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (dialogue != null)
        {
            dialogue.SetActive(false);
            activeDialogues.Remove(dialogue);
        }
        
        hideDialogueCoroutine = null;
    }

    void OnDestroy()
    {
        if (dialogue != null && activeDialogues.Contains(dialogue))
        {
            activeDialogues.Remove(dialogue);
        }
        
        if (hideDialogueCoroutine != null)
        {
            StopCoroutine(hideDialogueCoroutine);
        }
    }
}