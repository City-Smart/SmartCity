using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSign : MonoBehaviour
{
    private float rotation;
    private float signYRotation, oppositeRotation;

    private bool wrongDirection = false;
    private int timer = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer++;
    }

    void OnTriggerStay(Collider collision)
    {
        // Controlla il tag prima di procedere
        if (collision.gameObject.CompareTag("segnaletica"))
        {
            // Ottieni la rotazione dell'oggetto corrente e dell'oggetto in collisione
            rotation = transform.rotation.eulerAngles.y;
            signYRotation = collision.transform.rotation.eulerAngles.y;

            // Calcola la rotazione opposta normalizzata
            oppositeRotation = (signYRotation + 180f) % 360f;

            // Controlla se la rotazione dell'oggetto è opposta a quella della segnaletica
            if (Mathf.Abs(rotation - oppositeRotation) < 25f && timer >= 300)
            {
                timer = 0;
                Debug.Log("Senso di marcia sbagliato! Penalizzo l'agente");
            }
        }
    }
}