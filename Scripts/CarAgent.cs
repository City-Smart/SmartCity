using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CarAgent : Agent
{
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;
    
    public Transform frontLeftVisual;
    public Transform frontRightVisual;
    public Transform rearLeftVisual;
    public Transform rearRightVisual;
    
    public float motorForce = 200f;
    public float steeringAngle = 35f;
    public float checkDistance = 5.5f;
    public float angle = 0f;

    public Vector3 startingPosition, updatedPosition, targetPosition;
    public Transform targetObject;
    private UIManager managerUI;
    private Rigidbody rb;
    private bool parked = false;
    private bool isOnSidewalk, isNearPark, isOnRoad = false;

    private float rotation;
    private float signYRotation, oppositeRotation;
    private int timer = 0;

    private HashSet<int> rewardedObjectIds = new HashSet<int>();

    private float distanceToTarget, previousDistanceToTarget, fromStartToTarget;
    private float distanceReward, speedReward;
    private float acceleration, steering, breaking;
    private float rotationY = 0f;
    private float rewardD, angleReward, angleRad, angularReward;

    private Quaternion startingRotation;

    private GameObject[] cats, signals;
    private List<Transform> catsPositions = new List<Transform>();

    public Vector3 forward, directionToTarget;
    private bool isFacing = true;

    public float reward = 0;
    public float rewardDistance = 15f;
    public float rewardSpeed = 0.05f;
    public static int completedEpisodes = 0;
    public static float speed = 0f;

    private bool foundRoad = true;

    private float sameSteeringTime = 0f;
    public float steeringThreshold = 0.1f; // quanto deve essere diverso per considerarsi una nuova direzione
    public float maxSteeringTime = 3f;     // massimo tempo consentito in una direzione
    private float lastSteering = 0f;

    private GameObject showGlow = null;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = GameObject.FindGameObjectWithTag("inizio").transform.position;
        cats = GameObject.FindGameObjectsWithTag("gatto");
        signals = GameObject.FindGameObjectsWithTag("segnaletica");

        foreach (GameObject catPosition in cats) {
            catsPositions.Add(catPosition.transform);
        }

        showGlow = GameObject.FindGameObjectWithTag("bagliore");
        showGlow.GetComponent<MeshRenderer>().enabled = true; // Mostra il bagliore all'inizio ma solo dalla vista dall'alto
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void FixedUpdate() {
        ApplyBrakes();
        StabilizeCar();
    }

    private void UpdateWheelPose(WheelCollider collider, Transform visualTransform)
    {
        Vector3 position;
        Quaternion rotation;

        // Ottieni la posizione e la rotazione attuale del WheelCollider
        collider.GetWorldPose(out position, out rotation);

        // Sincronizza il modello visivo
        visualTransform.position = position;
        visualTransform.rotation = rotation;
    }

    private void ApplyBrakes() {
        float brake = 0;
        if (acceleration <= 0)
        {
            brake = Mathf.Lerp(0, 1500f, Time.fixedDeltaTime * 20f); // Applicazione graduale
        }
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
    }
    
    private void StabilizeCar() {
        float antiRollForce = 5000f; // Forza di stabilizzazione

        ApplyAntiRoll(frontLeftCollider, frontRightCollider, antiRollForce); // Assale anteriore
        ApplyAntiRoll(rearLeftCollider, rearRightCollider, antiRollForce); // Assale posteriore
    }

    private void ApplyAntiRoll(WheelCollider wheelL, WheelCollider wheelR, float antiRollForce) {
        WheelHit hit;

        float travelL = 1.0f;
        float travelR = 1.0f;

        if (wheelL.GetGroundHit(out hit)) {
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
        }
        if (wheelR.GetGroundHit(out hit)) {
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
        }

        float antiRoll = (travelL - travelR) * antiRollForce;

        if (wheelL.isGrounded) {
            rb.AddForceAtPosition(wheelL.transform.up * -antiRoll, wheelL.transform.position);
        }
        if (wheelR.isGrounded) {
            rb.AddForceAtPosition(wheelR.transform.up * antiRoll, wheelR.transform.position);
        }
    }

    GameObject GetClosestParkingSpot(GameObject pointOfInterest) {
        if (pointOfInterest == null) return null;

        GameObject[] parkingSpots = GameObject.FindGameObjectsWithTag("destinazione");
        if (parkingSpots.Length == 0) return null;

        GameObject closestSpot = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject spot in parkingSpots)
        {
            Transform interestDistance = pointOfInterest.transform;
            Vector3 groundDistance = new Vector3(interestDistance.position.x, 0, interestDistance.position.z);
            float distance = Vector3.Distance(groundDistance, spot.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSpot = spot;
            }
        }

        return closestSpot;
    }

    GameObject GetClosestObjectWithTag(string tag) {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        if (objects.Length == 0) return null;

        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }
        return closest;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("acqua"))
        {
            AddReward(-1500f);
            Debug.Log("Fuori dai limiti! Penalizzo l'agente");
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            AddReward(-35f);
            Debug.Log("Sul marciapiede! Penalizzo l'agente");
        }
        if (collision.gameObject.CompareTag("segnaletica"))
        {
            // Ottieni la rotazione dell'oggetto corrente e dell'oggetto in collisione
            rotation = transform.rotation.eulerAngles.y;
            signYRotation = collision.transform.rotation.eulerAngles.y;

            // Calcola la rotazione opposta normalizzata
            oppositeRotation = (signYRotation + 180f) % 360f;

            // Controlla se la rotazione dell'oggetto è opposta a quella della segnaletica
            if (Mathf.Abs(rotation - oppositeRotation) < 90f)
            {
                AddReward(-45f);
                Debug.Log("Senso di marcia sbagliato! Penalizzo l'agente");
                EndEpisode();
            }
            else
            {
                AddReward(3f);
                Debug.Log("Senso di marcia giusto! Ricompenso l'agente");
            }
        }
        if (collision.gameObject.CompareTag("obiettivo"))
        {
            AddReward(1000f);
            Debug.Log("Obiettivo raggiunto! Ricompenso l'agente");
            EndEpisode();
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            isOnSidewalk = true;
        }
        if (collision.gameObject.CompareTag("stradaPrivata"))
        {
            isNearPark = true;
        }
        if (collision.gameObject.CompareTag("strada"))
        {
            isOnRoad = true;
        }
    }
    
    void OnTriggerExit(Collider collision) 
    {
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            isOnSidewalk = false;
        }
        if (collision.gameObject.CompareTag("stradaPrivata"))
        {
            isNearPark = false;
        }
        if (collision.gameObject.CompareTag("strada"))
        {
            isOnRoad = false;
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.CompareTag("ostacolo") || collision.gameObject.CompareTag("marciapiede")) 
        {
            AddReward(-300f);
            Debug.Log("Ostacolo incontrato! Penalizzo l'agente");
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("macchina"))
        {
            AddReward(-300f);
            Debug.Log("Tamponamento! Penalizzo l'agente");
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("gatto"))
        {
            if (acceleration >= 0)
            {
                AddReward(-1350f * (acceleration + 1));
            }
            else
            {
                AddReward(-1350f);
            }
            Debug.Log("Gatto investito! Penalizzo l'agente");
            EndEpisode();
        }
    }

    public override void Initialize()
    {
        // Ottieni il riferimento al Rigidbody della macchina
        rb = GetComponent<Rigidbody>();

        // Salva la posizione iniziale e la rotazione della macchina
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        targetObject = GameObject.FindGameObjectWithTag("obiettivo").transform;
    }

    public override void OnEpisodeBegin()
    {
        completedEpisodes++;
        
        rewardedObjectIds.Clear();

        parked = false;
        
        sameSteeringTime = 0f;
        lastSteering = 0f;
        timer = 0;

        // Reset della posizione della macchina
        transform.position = startingPosition;
        transform.rotation = startingRotation;

        // Trova tutte le destinazioni
        GameObject[] pointOfInterest = GameObject.FindGameObjectsWithTag("puntoInteresse");

        // Scegli una destinazione casuale dalla lista
        GameObject randomTarget = pointOfInterest[Random.Range(0, pointOfInterest.Length)];

        // Aggiorna UI
        managerUI = FindObjectOfType<UIManager>();
        managerUI.UpdateUI(randomTarget);

        // Calcola il parcheggio più vicino al punto di interesse
        GameObject closestSpot = GetClosestParkingSpot(randomTarget);
        
        // Imposta la posizione del targetObject sul closestSpot
        targetObject.localPosition = closestSpot.transform.localPosition;
        targetPosition = targetObject.localPosition;

        // Resetta la velocità della macchina
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void Heuristic(in ActionBuffers actionBuffers)
    {
        ActionSegment<float> continuousActions = actionBuffers.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posizione relativa dell'obiettivo
        Vector3 relativeTargetPosition = targetPosition - updatedPosition;
        sensor.AddObservation(relativeTargetPosition.normalized.x); // Direzione
        sensor.AddObservation(relativeTargetPosition.normalized.z); // Direzione
        sensor.AddObservation(distanceToTarget); // Distanza

        sensor.AddObservation(timer);

        // Velocità della macchina
        sensor.AddObservation(rb.velocity.magnitude);

        // Controlla la posizione obiettivo
        sensor.AddObservation(targetPosition.normalized.x);
        sensor.AddObservation(targetPosition.normalized.z);

        // Orientamento della macchina
        sensor.AddObservation(transform.forward.normalized.x);
        sensor.AddObservation(transform.forward.normalized.z);

        // Rotazione della macchina
        sensor.AddObservation(transform.rotation.normalized.y);

        // Controllo del marciapiede
        sensor.AddObservation(isOnSidewalk ? 1f : 0f);

        // Trova il segnale più vicino
        Transform closestSignal = null;
        float signalDistance = Mathf.Infinity;

        foreach (var sign in signals)
        {
            float distance = Vector3.Distance(transform.position, sign.transform.position);
            if (distance < signalDistance)
            {
                signalDistance = distance;
                closestSignal = sign.transform;
            }
        }

        if (closestSignal != null)
        {
            // Aggiungi l'orientamento Y come osservazione
            float orientazioneY = closestSignal.transform.rotation.normalized.y;
            sensor.AddObservation(orientazioneY);
        }
        else
        {
            // Se non ci sono segnali, aggiungi un valore neutro
            sensor.AddObservation(0f);
        }

        // Trova il gatto più vicino
        Transform nearestCat = null;
        float minDistance = float.MaxValue;

        foreach (var cat in cats)
        {
            float distance = Vector3.Distance(transform.position, cat.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCat = cat.transform;
            }
        }

        if (nearestCat != null)
        {
            // Posizione relativa del gatto più vicino
            Vector3 relativePosition = nearestCat.position - transform.position;
            sensor.AddObservation(relativePosition.normalized.x);
            sensor.AddObservation(relativePosition.normalized.z);

            // Velocità e direzione del gatto
            Rigidbody catRb = nearestCat.GetComponent<Rigidbody>();
            sensor.AddObservation(catRb.velocity.normalized.x);
            sensor.AddObservation(catRb.velocity.normalized.z);
        }
        else
        {
            // Se non ci sono gatti visibili, aggiungi valori neutri
            sensor.AddObservation(0f); // Posizione x
            sensor.AddObservation(0f); // Posizione z
            sensor.AddObservation(0f); // Velocità x
            sensor.AddObservation(0f); // Velocità z
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Update function
        timer++;
        updatedPosition = GameObject.FindGameObjectWithTag("carTrigger").transform.position;
        distanceToTarget = Vector3.Distance(updatedPosition, targetPosition);
        fromStartToTarget = Vector3.Distance(startingPosition, targetPosition);
        speed = rb.velocity.magnitude;

        distanceReward = Mathf.Clamp01(1 - (distanceToTarget / fromStartToTarget));
        speedReward = Mathf.Clamp01(1f / rb.velocity.magnitude);

        forward = transform.forward;
        directionToTarget = (targetPosition - updatedPosition).normalized;
        angle = Vector3.SignedAngle(forward, directionToTarget, Vector3.up);
        isFacing = !(Mathf.Abs(angle) > 90f);

        // Trova la direzione di destra rispetto all'agente
        Vector3 rightDirection = transform.right; // Destra rispetto alla macchina

        // Calcola la posizione dove dovrebbe esserci un altro tassello strada
        Vector3 checkPosition = updatedPosition + (rightDirection * checkDistance);

        // Controlla se esiste un altro tassello con il tag "strada" in quella posizione
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 1f); // Controllo con una piccola tolleranza
        foundRoad = false;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("strada") || col.CompareTag("stradaPrivata"))
            {
                foundRoad = true;
                break;
            }
        }

        // Ottieni le azioni continue
        acceleration = actionBuffers.ContinuousActions[0]; // Azione per il movimento avanti/indietro
        steering = actionBuffers.ContinuousActions[1]; // Azione per la rotazione

        // Applica forza alle ruote posteriori per simulare il motore
        if (acceleration >= 0)
        {
            rearLeftCollider.motorTorque = acceleration * motorForce;
            rearRightCollider.motorTorque = acceleration * motorForce;
        }

        // Calcola l'angolo tra la direzione dell'auto e la direzione del target
        float angleToTarget = Vector3.Angle(transform.forward, targetObject.forward);

        angularReward = Mathf.Clamp01(angleToTarget / 180f);

        // Controlla se la direzione è la stessa (es. entrambe >0 o entrambe <0)
        if (Mathf.Abs(steering) > steeringThreshold && Mathf.Sign(steering) == Mathf.Sign(lastSteering))
        {
            sameSteeringTime += Time.deltaTime;
        }
        else
        {
            sameSteeringTime = 0f;
        }

        lastSteering = steering;

        // Penalità se sterza troppo a lungo nella stessa direzione
        if (sameSteeringTime > maxSteeringTime)
        {
            AddReward(-150f); // puoi regolare l’intensità
            Debug.Log("Sterzata nella stessa direzione per troppo tempo! Penalizzo l'agente");
            EndEpisode();
        }

        if (acceleration < 0)
            AddReward(3 * acceleration);

        if (rb.velocity.magnitude < 0.075f)
            AddReward(-0.5f);

        if (rb.velocity.magnitude < 2.5f)
            AddReward(-0.05f);

        if (rb.velocity.magnitude > 2f && !isOnSidewalk && foundRoad)
            AddReward(0.05f * rb.velocity.magnitude);

        if (!foundRoad)
            AddReward(-0.2f);

        if (!isOnSidewalk && acceleration > 0.1f && rb.velocity.magnitude > 2.5f && distanceToTarget < previousDistanceToTarget)
            AddReward(distanceReward);

        if (isNearPark && distanceToTarget > 2f && rb.velocity.magnitude > 1f && rb.velocity.magnitude < 0.05f)
            AddReward(speedReward);

        if (Mathf.FloorToInt(distanceToTarget) > previousDistanceToTarget)
            AddReward(-0.01f * distanceToTarget);

        if (!isFacing)
            AddReward(-2.5f);

        previousDistanceToTarget = distanceToTarget;

        if (distanceToTarget != 0 && distanceToTarget < 3f && rb.velocity.magnitude < 0.75f)
        {
            rotationY=transform.eulerAngles.y;
            angleRad=rotationY*Mathf.Deg2Rad;
            angleReward=Mathf.Abs(Mathf.Cos(2*angleRad));
            Debug.Log("Angolo rotazione parcheggio: " + angleReward);

            rewardD=Mathf.Clamp(500f/distanceToTarget,100,500);
            AddReward(Mathf.Clamp(rewardD*angleReward,50,500));
            Debug.Log("Ricompensa finale parcheggio: " + Mathf.Clamp(rewardD*angleReward,50,500));
            Debug.Log("Parcheggio completato!");
            EndEpisode();
        }

        // Applica l'angolo di sterzata alle ruote anteriori
        frontLeftCollider.steerAngle = steering * steeringAngle;
        frontRightCollider.steerAngle = steering * steeringAngle;

        // Aggiorna la posizione e rotazione delle ruote visive
        UpdateWheelPose(frontLeftCollider, frontLeftVisual);
        UpdateWheelPose(frontRightCollider, frontRightVisual);
        UpdateWheelPose(rearLeftCollider, rearLeftVisual);
        UpdateWheelPose(rearRightCollider, rearRightVisual);
    }
}