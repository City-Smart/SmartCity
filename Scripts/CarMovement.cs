using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody rb;                // Rigidbody per la fisica della macchina


    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    public float maxSteeringAngle = 30f; // Angolo massimo di sterzata
    public float motorForce = 1500f;      // Forza del motore
    public float brakeForce = 3000f;      // Forza dei freni
    public float acceleration = 0f;       // Accelerazione
    public float steering = 0f;           // Direzione di sterzata
    public float maxSpeed = 50f;          // Velocità massima
    private float currentSpeed = 0f;      // Velocità corrente


    void Update()
    {
        // Ottieni input di sterzata e accelerazione
        steering = Input.GetAxis("Horizontal") * maxSteeringAngle;
        acceleration = Input.GetAxis("Vertical");

        // Aggiorna l'angolo di sterzata delle ruote anteriori
        frontLeftTransform.localRotation = Quaternion.Euler(0f, steering, 0f);
        frontRightTransform.localRotation = Quaternion.Euler(0f, steering, 0f);

        // Applica accelerazione e frenata
        if (acceleration > 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * motorForce * Time.deltaTime, -maxSpeed, maxSpeed);
        }
        if (acceleration == 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed - ((motorForce * Time.deltaTime) / currentSpeed), 0, maxSpeed);
        }
        else if (acceleration < 0)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * brakeForce * Time.deltaTime, -maxSpeed, maxSpeed);
        }

        Vector3 forwardMovement = transform.forward * currentSpeed;
        rb.velocity = new Vector3(forwardMovement.x, rb.velocity.y, forwardMovement.z);


        // Limitare la velocità massima
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        // Gestire la rotazione del corpo della macchina (solo sterzata)
        float turnAmount = steering * Time.deltaTime * currentSpeed;
        transform.Rotate(0f, turnAmount, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Controlla se il GameObject con cui hai colliso ha il tag "marciapiede"
        if (collision.gameObject.CompareTag("marciapiede"))
        {
            Debug.Log("Fuori strada! Penalizzo l'agente");
        }

        if (collision.gameObject.CompareTag("macchina"))
        {
            Debug.Log("Tamponamento! Penalizzo l'agente");
        }

        if (collision.gameObject.CompareTag("parcheggio") && acceleration == 0)
        {
            Debug.Log("Parcheggio completato! Ricompenso l'agente");
        }
    }
}