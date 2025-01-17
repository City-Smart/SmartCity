using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPole : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab del GameObject da creare

    void Start()
    {
        // Trova tutti i GameObjects con il tag "parcheggio"
        GameObject[] lightObjects = GameObject.FindGameObjectsWithTag("semaforo");

        foreach (GameObject lightObject in lightObjects)
        {
            // Calcola il centro geometrico basato sulla MeshRenderer
            Vector3 basePosition = GetMeshCenter(lightObject);
            Quaternion baseRotation = lightObject.transform.rotation;

            // Calcola il vettore di spostamento basato sulla rotazione
            Vector3 shiftDirection = lightObject.transform.right; // Direzione verso sopra

            basePosition = basePosition + shiftDirection * 0.15f;

            // Crea i nuovi GameObjects
            Instantiate(objectPrefab, basePosition, baseRotation);
        }
    }

    // Metodo per calcolare il centro della MeshRenderer
    Vector3 GetMeshCenter(GameObject obj)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            // Ottieni il centro dei bounds della MeshRenderer
            return renderer.bounds.center;
        }

        // Se non ha una MeshRenderer, ritorna la posizione attuale del GameObject
        return obj.transform.position;
    }


}