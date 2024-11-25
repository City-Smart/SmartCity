using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public Transform frontLeftVisual;
    public Transform frontRightVisual;
    public Transform rearLeftVisual;
    public Transform rearRightVisual;

    public float motorForce = 1500f;
    public float steeringAngle = 30f;

    public bool parked = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Ottieni input
        float acceleration = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");   

        // Applica forza alle ruote posteriori per simulare il motore
        rearLeftCollider.motorTorque = acceleration * motorForce;
        rearRightCollider.motorTorque = acceleration * motorForce;

        // Applica l'angolo di sterzata alle ruote anteriori
        frontLeftCollider.steerAngle = steering * steeringAngle;
        frontRightCollider.steerAngle = steering * steeringAngle;

        if(acceleration == steering && acceleration != 0) 
        {
            
        }

        // Aggiorna la posizione e rotazione delle ruote visive
        UpdateWheelPose(frontLeftCollider, frontLeftVisual);
        UpdateWheelPose(frontRightCollider, frontRightVisual);
        UpdateWheelPose(rearLeftCollider, rearLeftVisual);
        UpdateWheelPose(rearRightCollider, rearRightVisual);
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("macchina"))
        {
            Debug.Log("Tamponamento! Penalizzo l'agente");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Ottieni la rotazione dell'oggetto con cui hai colliso
        Quaternion originalRotation = collision.transform.rotation;

        // Calcola il verso opposto della rotazione
        Quaternion oppositeRotation = Quaternion.Inverse(originalRotation);

        if (collision.gameObject.CompareTag("parcheggio") && !parked)
        {
            parked = true;
            Debug.Log("Parcheggio completato! Ricompenso l'agente");
        }
        if (collision.gameObject.CompareTag("segnaletica"))
        {
            Debug.Log("Senso di marcia sbagliato! Penalizzo l'agente");
        }
    }
}
