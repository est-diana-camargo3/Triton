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
        rb.useGravity = false;

        // Verificar referencias antes de suscribirse
        if (leftHand != null)
        {
            leftHand.OnStrokeDetected  += (power, dir) => ApplyStroke(power, dir, "Left");
           
            Debug.Log("[SWIM] Mano izquierda conectada");
        }
        else Debug.LogError("[SWIM] leftHand es NULL - arrastra el StrokeDetector en el inspector");

        if (rightHand != null)
        {
            rightHand.OnStrokeDetected += (power, dir) => ApplyStroke(power, dir, "Right");
            Debug.Log("[SWIM] Mano derecha conectada");
        }
        else Debug.LogError("[SWIM] rightHand es NULL - arrastra el StrokeDetector en el inspector");

        Debug.Log($"[SWIM] Rigidbody encontrado: {rb != null} | isKinematic: {rb.isKinematic}");
    }

    void ApplyStroke(float power, Vector3 strokeDirection, string side)
    {
        // Convertir dirección local del controller a espacio mundial
        Vector3 worldDirection = cameraRig.TransformDirection(strokeDirection);
    
        // Limitar cuánto afecta el componente vertical (opcional)
        worldDirection.y *= 0.3f;
        worldDirection.Normalize();

        Vector3 force = worldDirection * power * strokeForceMultiplier;
        rb.AddForce(force, ForceMode.Impulse);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;

        Debug.Log($"[SWIM] {side} | dir mundo: {worldDirection} | fuerza: {force}");
    }
    void Update()
    {
        currentSpeed = rb.velocity.magnitude;
        
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[SWIM] Posición player: {transform.position}  Velocidad: {currentSpeed:F2}");
        }
        
        // SOLO PARA TESTING - quitar antes de build final
// #if UNITY_EDITOR
//         if (Input.GetKeyDown(KeyCode.H)) ApplyStroke(2f, "Left");
//         if (Input.GetKeyDown(KeyCode.J)) ApplyStroke(2f, "Right");
//         if (Input.GetKeyDown(KeyCode.Space)) ApplyStroke(3f, "Both");
// #endif
    }
}