using UnityEngine;

public class AddCollidersToChildren : MonoBehaviour
{
    [ContextMenu("Aggiungi Colliders a Tutti i Figli")]
    void AddColliders()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.GetComponent<Collider>() == null && child.GetComponent<MeshFilter>() != null)
            {
                child.gameObject.AddComponent<MeshCollider>();
            }
        }
    }
}
