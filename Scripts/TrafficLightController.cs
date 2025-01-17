using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    private GameObject redLight;    // Luce rossa
    private GameObject yellowLight; // Luce gialla
    private GameObject greenLight;  // Luce verde
    private GameObject trigger;

    private float timer;            // Timer per il ciclo dei semafori
    public float redDuration = 5f;  // Durata della luce rossa
    public float yellowDuration = 3f; // Durata della luce gialla
    public float greenDuration = 10f; // Durata della luce verde

    private bool trafficLight = false;

    public enum LightState { Red, Yellow, Green }
    public LightState currentState; // Stato attuale del semaforo

    void Start()
    {
        // Trova le luci come figli del GameObject che possiede lo script
        redLight = FindChildWithTag("rosso");
        yellowLight = FindChildWithTag("giallo");
        greenLight = FindChildWithTag("verde");
        trigger = FindChildWithTag("triggerSemaforo");

        // Assicurati che tutte le luci siano trovate
        if (redLight == null || yellowLight == null || greenLight == null)
        {
            Debug.LogError("Una o più luci non sono state trovate come figli! Controlla i tag!");
            return;
        }

        // Inizializza il semaforo con lo stato rosso
        currentState = LightState.Red;
        timer = 0f;
        UpdateLights();
    }

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
                    UpdateLights();
                }
                break;

            case LightState.Green:
                if (timer >= greenDuration)
                {
                    currentState = LightState.Yellow;
                    timer = 0f;
                    UpdateLights();
                }
                break;

            case LightState.Yellow:
                if (timer >= yellowDuration)
                {
                    currentState = LightState.Red;
                    timer = 0f;
                    UpdateLights();
                }
                break;
        }
    }

    void UpdateLights()
    {
        // Attiva/disattiva le luci in base allo stato corrente
        redLight.SetActive(currentState == LightState.Red);
        yellowLight.SetActive(currentState == LightState.Yellow);
        greenLight.SetActive(currentState == LightState.Green);
    }

    // Funzione per trovare un figlio con un determinato tag
    GameObject FindChildWithTag(string tag)
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null; // Restituisce null se non trova il figlio con il tag specificato
    }
}