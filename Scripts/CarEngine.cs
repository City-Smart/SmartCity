using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarEngine : MonoBehaviour {
    public Transform path;
    public float maxSteerAngle = 45f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Rigidbody rb;

    public float motorForce = 150f;
    public float maxSpeed = 20f;

    private List<Transform> nodes;
    private int currentNode = 0;

    private void Start() {
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++) {
            if (pathTransforms[i] != path.transform) {
                nodes.Add(pathTransforms[i]);
            }
        }
    }

    private void FixedUpdate() {
        ApplyBrakes();
        StabilizeCar();
    }

    private void ApplySteer() {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position); 
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        wheelFL.steerAngle = newSteer;
        wheelFR.steerAngle = newSteer;
    }


    private void ApplyMotor() {
        if (rb.velocity.magnitude < maxSpeed) {
            wheelFL.motorTorque = motorForce;
            wheelFR.motorTorque = motorForce;
        } else {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
    }

    private void ApplyBrakes() {
        float brake = 0f;
        if (Input.GetKey(KeyCode.Space)) {
            brake = Mathf.Lerp(0, 1500f, Time.fixedDeltaTime * 5); // Applicazione graduale
        }

        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;
    }

    private void StabilizeCar() {
        float antiRollForce = 5000f; // Forza di stabilizzazione

        ApplyAntiRoll(wheelFL, wheelFR, antiRollForce); // Assale anteriore
        ApplyAntiRoll(wheelRL, wheelRR, antiRollForce); // Assale posteriore
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
}