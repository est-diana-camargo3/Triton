using UnityEngine;

/// <summary>
/// CAMBIO PRINCIPAL respecto a la versión anterior:
///
/// Antes: cameraRig.TransformDirection()  →  solo captura YAW (girar izq/der)
/// Ahora: centerEye.TransformDirection()  →  captura YAW + PITCH (mirar arriba/abajo)
///
/// Esto hace que la dirección del movimiento sea relativa a donde mira el jugador
/// en los 3 ejes, no solo en el plano horizontal.
///
/// MATEMÁTICAMENTE:
///   strokeDirection viene en espacio TrackingSpace (espacio "sala física")
///   centerEye.TransformDirection(v) aplica la rotación completa de la cabeza:
///     M_world = R_cameraRig × R_centerEye × v
///   donde R_centerEye incluye el pitch (rotación en X) que da la mirada vertical.
///
/// SETUP EN UNITY:
///   centerEye = OVRCameraRig → TrackingSpace → CenterEyeAnchor
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SwimmingController : MonoBehaviour
{
    [Header("Referencias")]
    public StrokeDetector leftHand;
    public StrokeDetector rightHand;

    [Tooltip("OVRCameraRig → TrackingSpace → CenterEyeAnchor")]
    public Transform centerEye;        // ← reemplaza a cameraRig

    [Header("Física del nado")]
    [Tooltip("Escala global de la fuerza")]
    public float strokeForceMultiplier = 8f;
    [Tooltip("Resistencia del agua")]
    public float waterDrag = 2.5f;
    [Tooltip("Velocidad máxima en m/s")]
    public float maxSpeed = 6f;

    [Header("Debug (solo lectura)")]
    public float currentSpeed;
    public bool  leftInPowerPhase;
    public bool  rightInPowerPhase;
    public Vector3 lastAppliedForce;

    private Rigidbody rb;

    void Start()
    {
        rb            = GetComponent<Rigidbody>();
        rb.drag       = waterDrag;
        rb.useGravity = false;

        if (leftHand  == null) Debug.LogError("[SWIM] leftHand es NULL");
        if (rightHand == null) Debug.LogError("[SWIM] rightHand es NULL");
        if (centerEye == null) Debug.LogError("[SWIM] centerEye es NULL — arrastra CenterEyeAnchor");

        Debug.Log($"[SWIM] isKinematic: {rb.isKinematic}");
    }

    void FixedUpdate()
    {
        ApplyHandForce(leftHand,  "Left");
        ApplyHandForce(rightHand, "Right");

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;

        currentSpeed      = rb.velocity.magnitude;
        leftInPowerPhase  = leftHand  != null && leftHand.isPowerPhase;
        rightInPowerPhase = rightHand != null && rightHand.isPowerPhase;
    }

    void ApplyHandForce(StrokeDetector hand, string side)
    {
        if (hand == null || !hand.isPowerPhase) return;

        // ── Transformación de dirección ────────────────────────────────────────
        // hand.currentStrokeDirection está en espacio TrackingSpace (sala física).
        // centerEye.TransformDirection() aplica la rotación completa de la cabeza
        // (yaw + pitch), convirtiendo ese vector al espacio mundo.
        //
        // Resultado: si el jugador mira 45° hacia arriba y empuja "adelante",
        // la dirección resultante en mundo apunta 45° hacia arriba-adelante.
        //
        // NO aplicamos ningún aplastamiento de Y — el movimiento es libre en
        // los 3 ejes igual que nadar en el océano real.
        Vector3 worldDirection = centerEye.TransformDirection(hand.currentStrokeDirection);
        worldDirection = worldDirection.normalized;

        // ForceMode.Force: fuerza continua por frame durante la fase de empuje
        // hand.currentForce = velocidad_brazo × multiplicador_de_carga
        Vector3 force = worldDirection * hand.currentForce * strokeForceMultiplier;
        rb.AddForce(force, ForceMode.Force);

        lastAppliedForce = force;

        if (Time.frameCount % 15 == 0)
            Debug.Log($"[SWIM] {side} | dir: {worldDirection:F2} | F: {force.magnitude:F2}N | vel: {currentSpeed:F2}");
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[SWIM] Pos: {transform.position} | Vel: {currentSpeed:F2}");
    }
}