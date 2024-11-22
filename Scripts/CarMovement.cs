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
}
