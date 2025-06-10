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

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>(); // key will get up and it will saved in "inventary"
    }

    void Update()
    {
        if ( NearView() && Input.GetKeyDown(KeyCode.E) )
        {
            if (isRedKey) player.RedKey = true;
            else player.BlueKey = true;
            Destroy(gameObject);
        }
    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
        direction = transform.position - player.transform.position;
        angleView = Vector3.Angle(player.transform.forward, direction);
        if (distance < 2f) return true; // angleView < 35f && 
        else return false;
    }
}
