using UnityEngine;

public class Shooting : MonoBehaviour


{
    public GameObject bullet; // Proiettile da istanziare

    // Update is called once per frame
    void Update()
    {
        // 1. Controllare se vine cliccato il mouse
        if (Input.GetButtonDown("Fire1"))
        {
            // 2. Se sì, creare un proiettile
            GameObject bulletInstance =  Instantiate(
                bullet, transform.position, transform.rotation);

            // 3. Assegnare la direzione del proiettile e una forza
            bulletInstance.GetComponent<Rigidbody>().AddForce(
                transform.forward * 100f, ForceMode.Impulse);
        }

        
    }
}
