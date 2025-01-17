using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightTrigger : MonoBehaviour
{
    public float timer;

    public float redDuration = 5f;  // Durata della luce rossa
    public float yellowDuration = 3f; // Durata della luce gialla
    public float greenDuration = 10f; // Durata della luce verde

    private bool trafficLight = false;

    public enum LightState { Red, Yellow, Green }
    public LightState currentState; // Stato attuale del semaforo

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
                // Aggiorna il timer
        timer += Time.deltaTime;

        // Controlla lo stato corrente e passa al successivo
        switch (currentState)
        {
            case LightState.Red:
                if (timer >= redDuration)
                {
                    currentState = LightState.Green;
                    timer = 0f;
                }
                break;

            case LightState.Green:
                if (timer >= greenDuration)
                {
                    currentState = LightState.Yellow;
                    timer = 0f;
                }
                break;

            case LightState.Yellow:
                if (timer >= yellowDuration)
                {
                    currentState = LightState.Red;
                    timer = 0f;
                }
                break;
        }
    }

    void OnTriggerEnter(Collider collision) 
    {
        // Controlla se il tag dell'oggetto è "player"
        if (collision.gameObject.CompareTag("Player") && !trafficLight)
        {
            trafficLight = true;
            // Controlla lo stato del semaforo
            if (currentState == LightState.Red)
            {
                Debug.Log("Semaforo rosso! Penalizzo l'agente");
            }
            if (currentState == LightState.Yellow)
            {
                Debug.Log("Semaforo giallo! Ricompenso lievemente l'agente");
            }
            else if (currentState == LightState.Green)
            {
                Debug.Log("Semaforo verde! Ricompenso l'agente");
            }
        }
    }
}
