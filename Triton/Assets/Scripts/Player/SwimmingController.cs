using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimmingController : MonoBehaviour
{
    [Header("Referencias")]
    public StrokeDetector leftHand;
    public StrokeDetector rightHand;

    [Tooltip("OVRCameraRig → TrackingSpace → CenterEyeAnchor")]
    public Transform centerEye;

    [Header("Física del nado")]
    public float strokeForceMultiplier = 8f;
    public float waterDrag             = 2.5f;
    public float maxSpeed              = 6f;

    [Header("Dirección")]
    [Tooltip("0 = dirección física pura | 1 = siempre donde mira la cabeza\n" +
             "El signo se preserva: brazada hacia atrás = moverse hacia atrás")]
    [Range(0f, 1f)]
    public float headInfluence = 0.5f;

    // ─────────────────────────────────────────────────────────────────────────
    // VFX
    // ─────────────────────────────────────────────────────────────────────────
    [Header("VFX")]
    [Tooltip("Trail de burbujas activo mientras el jugador se mueve")]
    public GameObject trailVFX;
    public GameObject trailVFX2;
    [Tooltip("Velocidad mínima para que el trail permanezca activo")]
    public float trailMinSpeed = 0.1f;

    // ─────────────────────────────────────────────────────────────────────────
    // DEBUG
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Debug (solo lectura)")]
    public float   currentSpeed;
    public bool    leftInPowerPhase;
    public bool    rightInPowerPhase;
    public Vector3 lastSwimDirection;

    private Rigidbody rb;
    private bool      wasMoving      = false;
    private bool      trailActive    = false;

    void Start()
    {
        rb            = GetComponent<Rigidbody>();
        rb.drag       = waterDrag;
        rb.useGravity = false;

        if (leftHand  == null) Debug.LogError("[SWIM] leftHand es NULL");
        if (rightHand == null) Debug.LogError("[SWIM] rightHand es NULL");
        if (centerEye == null) Debug.LogError("[SWIM] centerEye es NULL");

        SetVFX(trailVFX, false);
        SetVFX(trailVFX2, false);
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

        HandleTrailAndEndSFX();
    }

    void ApplyHandForce(StrokeDetector hand, string side)
    {
        if (hand == null || !hand.isPowerPhase) return;

        // ── Dirección corregida (sin doble rotación) ──────────────────────────
        //
        // physicalDir: viene en tracking space (= mundo cuando el rig no rota).
        //              Ya contiene la dirección correcta sea adelante o atrás.
        //
        // headDir: se orienta según el signo del dot product con physicalDir.
        //          Si la brazada va hacia adelante → headDir = centerEye.forward
        //          Si la brazada va hacia atrás    → headDir = -centerEye.forward
        //          Esto preserva la intención del jugador en ambas direcciones.
        //
        // Slerp mezcla ambos con headInfluence sin perder el signo.
        Vector3 physicalDir = hand.currentStrokeDirection;

        Vector3 headDir = Vector3.zero;
        if (centerEye != null)
        {
            float dot = Vector3.Dot(physicalDir, centerEye.forward);
            // Alinear headDir con la intención de la brazada (mismo hemisferio)
            headDir = dot >= 0 ? centerEye.forward : -centerEye.forward;
        }
        else
        {
            headDir = physicalDir;
        }

        Vector3 worldDirection = Vector3.Slerp(physicalDir, headDir, headInfluence).normalized;
        lastSwimDirection      = worldDirection;

        Vector3 force = worldDirection * hand.currentForce * strokeForceMultiplier;
        rb.AddForce(force, ForceMode.Force);

        if (Time.frameCount % 15 == 0)
            Debug.Log($"[SWIM] {side} | swimDir: {worldDirection:F2} | F: {force.magnitude:F2}N | vel: {currentSpeed:F2}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TRAIL VFX + SFX DE FIN DE MOVIMIENTO
    // ─────────────────────────────────────────────────────────────────────────
    void HandleTrailAndEndSFX()
    {
        bool isMoving = currentSpeed > trailMinSpeed;

        // Activar trail mientras se mueve
        if (isMoving && !trailActive)
        {
            SetVFX(trailVFX, true);
            SetVFX(trailVFX2, true);
            trailActive = true;
        }

        // Al detenerse: apagar trail, sonido de fin de movimiento y burbujas
        if (!isMoving && wasMoving)
        {
            SetVFX(trailVFX, false);
            SetVFX(trailVFX2, false);
            trailActive = false;

            // Apagar bubblesVFX de ambas manos al llegar a velocidad 0
            TurnOffHandBubbles(leftHand);
            TurnOffHandBubbles(rightHand);
            
        }

        wasMoving = isMoving;
    }

    void TurnOffHandBubbles(StrokeDetector hand)
    {
        if (hand != null && hand.bubblesVFX != null)
            hand.bubblesVFX.SetActive(false);
    }

    void SetVFX(GameObject vfx, bool active)
    {
        if (vfx != null) vfx.SetActive(active);
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[SWIM] Pos: {transform.position} | Vel: {currentSpeed:F2}");
    }
}