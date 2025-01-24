using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
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
    public float steeringAngle = 30f;


    public Vector3 startingPosition, updatedPosition, targetPosition;
    public Transform targetObject;
    private Rigidbody rb;
    private bool parked = false;
    private bool isOnSidewalk = false;

    private float rotation;
    private float signYRotation, oppositeRotation;
    private int timer = 0;

    private HashSet<int> rewardedObjectIds = new HashSet<int>();

    private float distanceToTarget, fromStartToTarget;
    private float acceleration, steering;

    private Quaternion startingRotation;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = GameObject.FindGameObjectWithTag("inizio").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        updatedPosition = GameObject.FindGameObjectWithTag("carTrigger").transform.position;
        distanceToTarget = Vector3.Distance(updatedPosition, targetObject.position);
        fromStartToTarget = Vector3.Distance(startingPosition, targetObject.position);

        // Penalità ogni mezzo secondo in cui l'agente non accelera
        if (timer % 30 == 0 && acceleration <=0.35f)
        {
            AddReward(-1f);
        }
    }
    
    private void FixedUpdate() {
        ApplyBrakes();
        StabilizeCar();
    }

    private void ApplyBrakes() {
        float brake = 0f;
        if (Input.GetKey(KeyCode.Space)) {
            brake = Mathf.Lerp(0, 1500f, Time.fixedDeltaTime * 5); // Applicazione graduale
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
    
    void OnTriggerEnter(Collider collision)
    {
        // Controlla se il GameObject con cui hai colliso ha il tag "marciapiede"
        if (collision.gameObject.CompareTag("marciapiede") && acceleration >= 0)
        {
            isOnSidewalk = true;
            AddReward(-5f * (acceleration + 1));
            Debug.Log("Fuori strada! Penalizzo l'agente");
        }
        if (collision.gameObject.CompareTag("gatto"))
        {
            AddReward(-60f);
            Debug.Log("Gatto investito! Penalizzo l'agente");
        }
        if (collision.gameObject.CompareTag("acqua"))
        {
            AddReward(-100f);
            Debug.Log("In acqua! Penalizzo l'agente");
            EndEpisode();
        }
        int objectId = collision.gameObject.GetInstanceID();

        // Check if this object has already been rewarded
        if (rewardedObjectIds.Contains(objectId))
        {
            return; // Exit if already rewarded
        }

        rewardedObjectIds.Add(objectId);

        if (collision.gameObject.CompareTag("strada") && acceleration >= 0.75f)
        {
            AddReward(3f);
        }
    }

    void OnTriggerExit(Collider collision) {
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            isOnSidewalk = false;
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (rb.velocity.magnitude < 0.01f && !parked && distanceToTarget <= 3.5f)
        {
            parked = true;
            Debug.Log("Parcheggio completato! Ricompenso l'agente");
            AddReward(100f);
            EndEpisode();
        }
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
                AddReward(-30f);
                Debug.Log("Senso di marcia sbagliato! Penalizzo l'agente");
            }
        }
        if (collision.gameObject.CompareTag("macchina"))
        {
            AddReward(-80f);
            Debug.Log("Tamponamento! Penalizzo l'agente");
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.CompareTag("ostacolo")) 
        {
            AddReward(-50f);
            Debug.Log("Ostacolo incontrato! Penalizzo l'agente");
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("macchina"))
        {
            AddReward(-80f);
            Debug.Log("Tamponamento! Penalizzo l'agente");
            EndEpisode();
        }
    }

    public override void Initialize()
    {
        // Ottieni il riferimento al Rigidbody della macchina
        rb = GetComponent<Rigidbody>();

        // Salva la posizione iniziale e la rotazione della macchina
        startingPosition = transform.position;
        startingRotation = Quaternion.identity;

        targetObject = GameObject.FindGameObjectWithTag("obiettivo").transform;
    }

    public override void OnEpisodeBegin()
    {
        rewardedObjectIds.Clear();

        parked = false;

        // Reset della posizione della macchina
        transform.position = startingPosition;
        transform.rotation = startingRotation;

        // Trova tutte le destinazioni
        GameObject[] destinationObjects = GameObject.FindGameObjectsWithTag("destinazione");

        // Scegli una destinazione casuale dalla lista
        GameObject randomTarget = destinationObjects[Random.Range(0, destinationObjects.Length)];

        // Imposta la posizione del targetObject sulla destinazione casuale
        targetObject.localPosition = randomTarget.transform.localPosition;

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
        Vector3 relativeTargetPosition = targetObject.localPosition - transform.localPosition;
        sensor.AddObservation(relativeTargetPosition.normalized); // Direzione
        sensor.AddObservation(relativeTargetPosition.magnitude); // Distanza

        // Orientamento della macchina
        sensor.AddObservation(transform.forward);

        // Controlla se è sul marciapiede
        sensor.AddObservation(isOnSidewalk ? 1.0f : 0.0f);
    
        // Controlla la posizione obiettivo
        sensor.AddObservation(targetPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Ottieni le azioni continue
        acceleration = actionBuffers.ContinuousActions[0]; // Azione per il movimento avanti/indietro
        steering = actionBuffers.ContinuousActions[1]; // Azione per la rotazione

        // Applica forza alle ruote posteriori per simulare il motore
        rearLeftCollider.motorTorque = acceleration * motorForce;
        rearRightCollider.motorTorque = acceleration * motorForce;
        
        if (steering != 0)
        {
            AddReward(-0.05f);
        }
        if (steering != 0 && acceleration >= 0)
        {
            AddReward(-0.05f);
        }
        if (acceleration >= 0.7f) 
        {
            AddReward(0.0001f);
        }
        
        // Debug.Log(rearRightCollider.motorTorque + " " + acceleration);

        // Applica l'angolo di sterzata alle ruote anteriori
        frontLeftCollider.steerAngle = steering * steeringAngle;
        frontRightCollider.steerAngle = steering * steeringAngle;

        // Aggiorna la posizione e rotazione delle ruote visive
        UpdateWheelPose(frontLeftCollider, frontLeftVisual);
        UpdateWheelPose(frontRightCollider, frontRightVisual);
        UpdateWheelPose(rearLeftCollider, rearLeftVisual);
        UpdateWheelPose(rearRightCollider, rearRightVisual);

        // Ricompensa per avvicinarsi all'obiettivo
        // AddReward(-distanceToTarget * 0.001f);
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
}