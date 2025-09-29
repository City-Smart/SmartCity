using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

public class UIManager : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public Button switchButton;
    public TextMeshProUGUI episodeText, speedText;

    private int currentCameraIndex = 0;
    private Camera[] cameras;

    private float speedKmh;

    private static Dictionary<string, string> objectToPlace = new Dictionary<string, string>
    {
        { "Bottle", "Enoteca" },
        { "BowlingBall", "Bowling" },
        { "Burger", "Paninoteca" },
        { "Coffee", "Bar" },
        { "Guitar", "Karaoke" },
        { "Hotdog", "Chiosco di Hotdog" },
        { "Lollypop", "Negozio di Caramelle" },
        { "Milkshake", "Gelateria" },
        { "Noodles", "Giapponese" },
        { "Pizza", "Pizzeria" },
        { "Popcorn", "Cinema" },
        { "Soda", "Lounge Bar" },
        { "Taco", "Messicano" }
    };

    void Start()
    {
        // Inizializza l'array delle camere
        cameras = new Camera[] { camera1, camera2, camera3 };

        // Attiva solo la prima camera
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
        // Assegna il pulsante alla funzione di cambio camera
        switchButton.onClick.AddListener(SwitchCamera);
    }

    void Update()
    {
        episodeText.text = "Completed Episodes: " + CarAgent.completedEpisodes;
        speedKmh = CarAgent.speed * 3.6f;
        speedText.text = speedKmh.ToString("F1") + " km/h";
    }

    void SwitchCamera()
    {
        // Disattiva la camera attuale
        cameras[currentCameraIndex].gameObject.SetActive(false);
        // Passa alla prossima camera
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        // Attiva la nuova camera
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    public static string ProcessString(string input)
    {
        string withoutNumbers = Regex.Replace(input, @"\d+$", "");
        string withoutPrefix = withoutNumbers.Replace("SM_Prop_LargeSign", "");
        string key = withoutPrefix.Replace("_", "");
        return objectToPlace.TryGetValue(key, out string place) ? place : "Località non trovata";
    }

    public void UpdateUI(GameObject randomTarget)
    {
        GameObject addressTextObject = GameObject.FindWithTag("indirizzo");

        if (addressTextObject != null)
        {
            TextMeshProUGUI textComponent = addressTextObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = ProcessString(randomTarget.name);
            }
            else
            {
                Debug.LogWarning("Il GameObject con tag 'indirizzo' non ha un componente TextMeshProUGUI!");
            }
        }
        else
        {
            Debug.LogWarning("Nessun GameObject con tag 'indirizzo' trovato nella scena!");
        }
    }
}