using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parking : MonoBehaviour
{
    public Rigidbody rb;

    private bool parkedAlready = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collision)
    {
        // Controlla se il GameObject con cui hai colliso ha il tag "marciapiede"
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            Debug.Log("Fuori strada! Penalizzo l'agente");
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("parcheggio") && rb.velocity.magnitude < 0.01f && !parkedAlready)
        {
            parkedAlready = true;
            Debug.Log("Parcheggio completato! Ricompenso l'agente");
        }
    }
}