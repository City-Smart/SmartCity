using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public float distanzaMovimento = 15f; // Distanza di movimento
    public float velocitàMovimento = 2f; // Velocità del movimento
    public float intervalloMovimento = 10f; // Intervallo in secondi tra i movimenti

    private bool staMuovendosi = false; // Per evitare movimenti multipli contemporaneamente
    private Animator animatore; // Riferimento all'Animator



    void Start()
    {
        animatore = GetComponent<Animator>(); // Ottieni il componente Animator
        StartCoroutine(GestioneMovimenti()); // Avvia la routine per il movimento automatico
    }

    IEnumerator GestioneMovimenti()
    {
        while (true) // Ciclo infinito per il movimento
        {
            yield return new WaitForSeconds(intervalloMovimento); // Aspetta l'intervallo specificato
            if (!staMuovendosi) // Muovi solo se non sta già muovendosi
            {
                CalcolaDistanza();
            }
        }
    }

    void CalcolaDistanza()
    {
        // Genera una direzione casuale in 2D (X, Z)
        Vector3 direzioneCasuale = new Vector3(
            Random.Range(-1, 2), // -1, 0 o 1
            0,
            Random.Range(-1, 2)  // -1, 0 o 1
        ).normalized;

        // Calcola la nuova posizione
        Vector3 posizioneTarget = transform.position + transform.forward * distanzaMovimento;

        // Esegui un Raycast per verificare il terreno
        RaycastHit hit;
        if (Physics.Raycast(posizioneTarget + Vector3.up * 10f, Vector3.down, out hit, 20f)) // Raycast dall'alto
        {
            if (hit.collider.CompareTag("marciapiede")) // Controlla se è "marciapiede"
            {
                StartCoroutine(Muovi(posizioneTarget)); // Avvia il movimento fluido
            }
            else
            {
                CalcolaDistanza(); // Cerca una nuova posizione
            }
        }
        else
        {
            Debug.Log("Nessun terreno trovato sotto la posizione target.");
        }
    }

    IEnumerator Muovi(Vector3 posizioneTarget)
    {
        staMuovendosi = true;

        Vector3 posizioneIniziale = transform.position;
        float tempoTrascorso = 0f;

        // Calcola la durata del movimento in base alla velocità
        float durataPercorso = Vector3.Distance(posizioneIniziale, posizioneTarget) / velocitàMovimento;

        // Ruota il gatto verso la direzione del movimento
        Vector3 direzioneMovimento = (posizioneTarget - posizioneIniziale).normalized;
        transform.rotation = Quaternion.LookRotation(direzioneMovimento);

        // Ciclo per ripetere l'animazione
        if (animatore != null)
        {
            Debug.Log("Inizio animazione walk");
            StartCoroutine(Camminata());
        }

        while (tempoTrascorso < durataPercorso)
        {
            transform.position = Vector3.Lerp(posizioneIniziale, posizioneTarget, tempoTrascorso / durataPercorso);
            tempoTrascorso += Time.deltaTime;
            yield return null; // Aspetta il prossimo frame
        }

        transform.position = posizioneTarget; // Assicurati che la posizione finale sia esatta

        // Ferma l'animazione di camminata
        if (animatore != null)
        {
            StopCoroutine(Camminata()); // Ferma la ripetizione
            Debug.Log("Ritorno all'animazione sit");
            animatore.Play("sit"); // Sostituisci "sit" con il nome dello stato di animazione sit
        }

        // Ruota il gatto di 180 gradi
        transform.Rotate(0, 180, 0);

        staMuovendosi = false;
    }

    IEnumerator Camminata()
    {
        while (true) // Ciclo infinito per ripetere l'animazione
        {
            animatore.Play("walk");
            yield return new WaitForSeconds(2f); // Aspetta la fine dell'animazione
        }
    }
}