using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destinations : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab del GameObject da creare

    void Start()
    {
        // Trova tutti i GameObjects con il tag "parcheggio"
        GameObject[] parkingObjects = GameObject.FindGameObjectsWithTag("parcheggio");

        foreach (GameObject parkingObject in parkingObjects)
        {
            // Calcola il centro geometrico basato sulla MeshRenderer
            Vector3 basePosition = GetMeshCenter(parkingObject);
            Quaternion baseRotation = parkingObject.transform.rotation;

            // Calcola il vettore di spostamento basato sulla rotazione
            Vector3 shiftDirection = parkingObject.transform.right; // Direzione verso destra

            // Posizioni per i nuovi oggetti
            Vector3 leftPosition = basePosition - shiftDirection * 3.75f;
            Vector3 rightPosition = basePosition + shiftDirection * 3.75f;

            // Crea i nuovi GameObjects
            Instantiate(objectPrefab, basePosition, baseRotation); // Al centro
            Instantiate(objectPrefab, leftPosition, baseRotation); // A sinistra
            Instantiate(objectPrefab, rightPosition, baseRotation); // A destra
        }
        Destroy(objectPrefab); 
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
