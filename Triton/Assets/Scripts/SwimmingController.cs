using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimmingController : MonoBehaviour
{
    [Header("Referencias")]
    public StrokeDetector leftHand;
    public StrokeDetector rightHand;
    public Transform cameraRig;          // OVRCameraRig

    [Header("Física del nado")]
    public float strokeForceMultiplier = 3f;
    public float waterDrag = 2.5f;
    public float maxSpeed = 5f;

    [Header("Debug (read only)")]
    public float currentSpeed;
    public int totalStrokes;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = waterDrag;
        rb.useGravity = false;   // estamos "flotando"

        // Suscribirse a eventos de cada mano
        leftHand.OnStrokeDetected  += (power) => ApplyStroke(power, "Left");
        rightHand.OnStrokeDetected += (power) => ApplyStroke(power, "Right");
    }

    void ApplyStroke(float power, string side)
    {
        // La dirección del impulso es hacia donde mira la cámara
        Vector3 direction = cameraRig.forward;
        direction.y *= 0.3f;   // reducir componente vertical para que no suba mucho
        direction.Normalize();

        Vector3 force = direction * power * strokeForceMultiplier;
        rb.AddForce(force, ForceMode.Impulse);

        // Limitar velocidad máxima
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;

        totalStrokes++;
        Debug.Log($"Brazada {side} | Power: {power:F2} | Velocidad actual: {rb.velocity.magnitude:F2}");
    }

    void Update()
    {
        currentSpeed = rb.velocity.magnitude;
        
        // SOLO PARA TESTING - quitar antes de build final
// #if UNITY_EDITOR
//         if (Input.GetKeyDown(KeyCode.H)) ApplyStroke(2f, "Left");
//         if (Input.GetKeyDown(KeyCode.J)) ApplyStroke(2f, "Right");
//         if (Input.GetKeyDown(KeyCode.Space)) ApplyStroke(3f, "Both");
// #endif
    }
}