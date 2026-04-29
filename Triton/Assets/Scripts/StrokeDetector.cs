using UnityEngine;

/// <summary>
/// Detecta la fase de empuje de una brazada y expone fuerza/dirección continua
/// para que SwimmingController la aplique cada FixedUpdate.
///
/// FLUJO:
///   1. Presionar gatillo  → inicia carga y registra posición inicial
///   2. Mover brazo        → si el movimiento se alinea con la dirección acumulada
///                           de la brazada, entra en FASE DE EMPUJE
///   3. Durante el empuje  → isPowerPhase = true, SwimmingController aplica fuerza
///                           continua proporcional a: carga × velocidad del brazo
///   4. Brazo vuelve       → dot product cae bajo el umbral → FASE DE RECOBRO
///                           isPowerPhase = false, no se aplica fuerza
///   5. Soltar gatillo     → resetea todo
/// </summary>
public class StrokeDetector : MonoBehaviour
{
    public enum HandSide { Left, Right }
    public HandSide hand;

    // ─────────────────────────────────────────────────────────────────────────
    // DETECCIÓN DE FASE DE EMPUJE
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Detección de empuje")]
    [Tooltip("Velocidad mínima del controller para considerar que hay empuje activo (m/s)")]
    public float minStrokeVelocity = 0.3f;

    [Tooltip("Qué tan alineado debe estar el movimiento con la dirección de la brazada " +
             "para entrar en fase de empuje. 0 = cualquier dirección, 0.5 = ±60°")]
    [Range(0f, 1f)]
    public float alignmentThreshold = 0.3f;

    // ─────────────────────────────────────────────────────────────────────────
    // CARGA
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Carga del impulso")]
    [Tooltip("Tiempo para alcanzar el 100% de carga (segundos)")]
    public float maxChargeTime = 2f;
    [Tooltip("Multiplicador de fuerza con 0% de carga")]
    public float minChargeMultiplier = 0.3f;
    [Tooltip("Multiplicador de fuerza con 100% de carga")]
    public float maxChargeMultiplier = 1f;

    // ─────────────────────────────────────────────────────────────────────────
    // COOLDOWN (evita detectar recobro como nueva brazada inmediatamente)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Cooldown entre brazadas")]
    public float strokeCooldown = 0.25f;
    private float lastStrokeEndTime;

    // ─────────────────────────────────────────────────────────────────────────
    // DEBUG
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Debug (solo lectura)")]
    public bool  isPowerPhase;
    public float currentForce;
    public float currentAlignment;
    [Range(0f, 100f)]
    public float currentChargePercent;
    public float strokeDistance;
    public string currentPhase;

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADO PÚBLICO que lee SwimmingController cada FixedUpdate
    // ─────────────────────────────────────────────────────────────────────────
    [HideInInspector] public Vector3 currentStrokeDirection;

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────────────────
    private OVRInput.Controller controller;
    private bool    isCharging       = false;
    private float   triggerPressTime = -1f;
    private Vector3 startPosition;
    private Vector3 previousVelocity;

    void Start()
    {
        controller = (hand == HandSide.Left)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;
    }

    void Update()
    {
        float   trigger        = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        bool    isTriggered    = trigger > 0.8f;
        Vector3 controllerVel  = OVRInput.GetLocalControllerVelocity(controller);
        Vector3 controllerPos  = OVRInput.GetLocalControllerPosition(controller);

        if (isTriggered)
        {
            // ── Inicio de carga ───────────────────────────────────────────────
            if (!isCharging)
            {
                isCharging       = true;
                triggerPressTime = Time.time;
                startPosition    = controllerPos;
                previousVelocity = controllerVel;
                isPowerPhase     = false;
            }

            // ── Carga acumulada ───────────────────────────────────────────────
            float elapsed        = Time.time - triggerPressTime;
            currentChargePercent = Mathf.Clamp01(elapsed / maxChargeTime) * 100f;
            float chargeRatio    = currentChargePercent / 100f;
            float chargeMult     = Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, chargeRatio);

            // ── Dirección acumulada de la brazada (delta de posición) ─────────
            // strokeVector: de donde empezó la mano a donde está ahora
            Vector3 strokeVector    = controllerPos - startPosition;
            strokeDistance          = strokeVector.magnitude;

            // currentStrokeDirection: dirección OPUESTA al movimiento de la mano
            // (empujas el agua hacia atrás → te mueves en dirección contraria)
            if (strokeVector.magnitude > 0.01f)
                currentStrokeDirection = -strokeVector.normalized;

            // ── Detección de fase: dot product entre vel actual y dir de empuje ─
            // Si la mano se mueve en la misma dirección que strokeVector
            // (es decir, sigue empujando) → dot product positivo
            float speed = controllerVel.magnitude;
            currentAlignment = speed > 0.01f
                ? Vector3.Dot(controllerVel.normalized, strokeVector.normalized)
                : 0f;

            bool cooldownOk   = Time.time - lastStrokeEndTime > strokeCooldown;
            bool movingEnough = speed > minStrokeVelocity;
            bool aligned      = currentAlignment > alignmentThreshold;
            bool hasTraveled  = strokeDistance > 0.05f; // mínimo recorrido para no disparar con tembleos

            // ── FASE DE EMPUJE ────────────────────────────────────────────────
            if (movingEnough && aligned && hasTraveled && cooldownOk)
            {
                isPowerPhase = true;

                // Fuerza = velocidad del brazo × multiplicador de carga
                // SwimmingController multiplica esto por su propio strokeForceMultiplier
                currentForce = speed * chargeMult;
                currentPhase = $"EMPUJE | vel: {speed:F2} | carga: {currentChargePercent:F0}%";

                ApplyChargingHaptic(chargeRatio);
            }
            // ── FASE DE RECOBRO ───────────────────────────────────────────────
            else
            {
                if (isPowerPhase)
                {
                    // Acaba de salir de la fase de empuje
                    lastStrokeEndTime = Time.time;
                    TriggerStrokeEndHaptic(chargeRatio);

                    // Resetear posición de inicio para la próxima brazada
                    // sin soltar el gatillo
                    startPosition    = controllerPos;
                    strokeDistance   = 0f;
                }

                isPowerPhase = false;
                currentForce = 0f;
                currentPhase = movingEnough
                    ? $"Recobro | alineación: {currentAlignment:F2}"
                    : $"Cargando: {currentChargePercent:F0}%";

                if (!movingEnough)
                    ApplyChargingHaptic(chargeRatio);
                else
                    StopHaptics();
            }

            previousVelocity = controllerVel;
        }
        // ── GATILLO SOLTADO ───────────────────────────────────────────────────
        else
        {
            if (isCharging)
            {
                StopHaptics();
                isCharging           = false;
                triggerPressTime     = -1f;
                currentChargePercent = 0f;
                strokeDistance       = 0f;
                isPowerPhase         = false;
                currentForce         = 0f;
            }
            currentPhase     = "Gatillo no presionado";
            currentAlignment = 0f;
            previousVelocity = Vector3.zero;
        }

        if (Time.frameCount % 30 == 0)
            Debug.Log($"[STROKE-{hand}] {currentPhase} | dist: {strokeDistance:F2}m");
    }

    // Vibración pulsante que sube mientras carga (solo cuando NO está empujando)
    void ApplyChargingHaptic(float chargeRatio)
    {
#if !UNITY_EDITOR
        float amplitude  = Mathf.Lerp(0.05f, 0.45f, chargeRatio);
        float pulseSpeed = Mathf.Lerp(3f, 10f, chargeRatio);
        float pulse      = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        OVRInput.SetControllerVibration(amplitude * pulse, amplitude * pulse, controller);
#endif
    }

    // Golpe corto al terminar la fase de empuje (confirmación)
    void TriggerStrokeEndHaptic(float chargeRatio)
    {
#if !UNITY_EDITOR
        float amplitude = Mathf.Lerp(0.2f, 0.8f, chargeRatio);
        OVRInput.SetControllerVibration(amplitude, amplitude, controller);
        Invoke(nameof(StopHaptics), 0.1f);
#endif
        Debug.Log($"[STROKE-{hand}] Fin de empuje | carga: {chargeRatio * 100f:F0}% | dist: {strokeDistance:F2}m");
    }

    void StopHaptics()
    {
#if !UNITY_EDITOR
        OVRInput.SetControllerVibration(0, 0, controller);
#endif
    }
}