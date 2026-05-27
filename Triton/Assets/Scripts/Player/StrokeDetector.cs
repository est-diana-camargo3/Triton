using UnityEngine;

public class StrokeDetector : MonoBehaviour
{
    public enum HandSide { Left, Right }
    public HandSide hand;

    // ─────────────────────────────────────────────────────────────────────────
    // DETECCIÓN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Detección de empuje")]
    public float minStrokeVelocity  = 0.3f;
    [Range(0f, 1f)]
    public float alignmentThreshold = 0.3f;

    // ─────────────────────────────────────────────────────────────────────────
    // CARGA
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Carga del impulso")]
    public float maxChargeTime        = 2f;
    public float minChargeMultiplier  = 0.3f;
    public float maxChargeMultiplier  = 1f;

    // ─────────────────────────────────────────────────────────────────────────
    // COOLDOWN
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Cooldown entre brazadas")]
    public float strokeCooldown = 0.25f;
    private float lastStrokeEndTime;

    // ─────────────────────────────────────────────────────────────────────────
    // VFX
    // ─────────────────────────────────────────────────────────────────────────
    [Header("VFX")]
    [Tooltip("Aparece mientras se está cargando la brazada")]
    public GameObject chargingVFX;

    [Tooltip("Flash corto al iniciar el movimiento de brazada")]
    public GameObject explosionVFX;
    [Tooltip("Duración del explosionVFX en segundos")]
    public float explosionDuration = 0.3f;

    [Tooltip("Burbujas que aparecen al iniciar el movimiento")]
    public GameObject bubblesVFX;

    // ─────────────────────────────────────────────────────────────────────────
    // DEBUG
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Debug (solo lectura)")]
    public bool   isPowerPhase;
    public float  currentForce;
    public float  currentAlignment;
    [Range(0f, 100f)]
    public float  currentChargePercent;
    public float  strokeDistance;
    public string currentPhase;

    [HideInInspector] public Vector3 currentStrokeDirection;

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────────────────
    private OVRInput.Controller controller;
    private bool    isCharging       = false;
    private float   triggerPressTime = -1f;
    private Vector3 startPosition;
    private Vector3 previousVelocity;
    private bool    chargingSFXPlaying = false;

    void Start()
    {
        controller = (hand == HandSide.Left)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;

        SetVFX(chargingVFX,  false);
        SetVFX(explosionVFX, false);
        SetVFX(bubblesVFX,   false);
    }

    void Update()
    {
        float   trigger       = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        bool    isTriggered   = trigger > 0.8f;
        Vector3 controllerVel = OVRInput.GetLocalControllerVelocity(controller);
        Vector3 controllerPos = OVRInput.GetLocalControllerPosition(controller);

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

                SetVFX(chargingVFX, true);
                chargingSFXPlaying = false;
            }

            // ── Carga acumulada ───────────────────────────────────────────────
            float elapsed        = Time.time - triggerPressTime;
            currentChargePercent = Mathf.Clamp01(elapsed / maxChargeTime) * 100f;
            float chargeRatio    = currentChargePercent / 100f;
            float chargeMult     = Mathf.Lerp(minChargeMultiplier, maxChargeMultiplier, chargeRatio);

            // SFX de carga (una sola vez al empezar)
            if (!chargingSFXPlaying)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cargandoBrazada, transform.position);
                chargingSFXPlaying = true;
            }

            // ── Dirección acumulada ───────────────────────────────────────────
            Vector3 strokeVector = controllerPos - startPosition;
            strokeDistance = strokeVector.magnitude;

            if (strokeVector.magnitude > 0.01f)
                currentStrokeDirection = -strokeVector.normalized;

            // ── Dot product: fase de empuje ───────────────────────────────────
            float speed      = controllerVel.magnitude;
            currentAlignment = speed > 0.01f
                ? Vector3.Dot(controllerVel.normalized, strokeVector.normalized)
                : 0f;

            bool cooldownOk    = Time.time - lastStrokeEndTime > strokeCooldown;
            bool movingEnough  = speed > minStrokeVelocity;
            bool aligned       = currentAlignment > alignmentThreshold;
            bool hasTraveled   = strokeDistance > 0.05f;

            // ── FASE DE EMPUJE ────────────────────────────────────────────────
            if (movingEnough && aligned && hasTraveled && cooldownOk)
            {
                if (!isPowerPhase)
                {
                    // Acaba de entrar en fase de empuje: activar VFX de explosión y burbujas
                    SetVFX(chargingVFX,  false);
                    SetVFX(explosionVFX, true);
                    SetVFX(bubblesVFX,   true);

                    // SFX de brazada al iniciar el movimiento
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.brazadas, transform.position);

                    // Apagar explosion después de su duración
                    Invoke(nameof(HideExplosionVFX), explosionDuration);
                }

                isPowerPhase = true;
                currentForce = speed * chargeMult;
                currentPhase = $"EMPUJE | vel: {speed:F2} | carga: {currentChargePercent:F0}%";

                ApplyChargingHaptic(chargeRatio);
            }
            // ── FASE DE RECOBRO ───────────────────────────────────────────────
            else
            {
                if (isPowerPhase)
                {
                    lastStrokeEndTime = Time.time;
                    TriggerStrokeEndHaptic(chargeRatio);

                    // Resetear posición para siguiente brazada sin soltar gatillo
                    startPosition  = controllerPos;
                    strokeDistance = 0f;

                    // Burbujas se apagan: SwimmingController decide cuándo via velocidad
                    // La explosion ya se apagó sola con el Invoke
                }

                isPowerPhase = false;
                currentForce = 0f;
                currentPhase = movingEnough
                    ? $"Recobro | alineación: {currentAlignment:F2}"
                    : $"Cargando: {currentChargePercent:F0}%";

                // Volver a mostrar chargingVFX si ya no está en empuje
                if (!isPowerPhase && isCharging)
                    SetVFX(chargingVFX, true);

                if (!movingEnough) ApplyChargingHaptic(chargeRatio);
                else               StopHaptics();
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
                chargingSFXPlaying   = false;

                SetVFX(chargingVFX,  false);
                SetVFX(bubblesVFX,   false);
                CancelInvoke(nameof(HideExplosionVFX));
                SetVFX(explosionVFX, false);
            }

            currentPhase     = "Gatillo no presionado";
            currentAlignment = 0f;
            previousVelocity = Vector3.zero;
        }

        if (Time.frameCount % 30 == 0)
            Debug.Log($"[STROKE-{hand}] {currentPhase} | dist: {strokeDistance:F2}m");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    void SetVFX(GameObject vfx, bool active)
    {
        if (vfx != null) vfx.SetActive(active);
    }

    void HideExplosionVFX() => SetVFX(explosionVFX, false);

    void ApplyChargingHaptic(float chargeRatio)
    {
#if !UNITY_EDITOR
        float amplitude  = Mathf.Lerp(0.05f, 0.45f, chargeRatio);
        float pulseSpeed = Mathf.Lerp(3f, 10f, chargeRatio);
        float pulse      = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        OVRInput.SetControllerVibration(amplitude * pulse, amplitude * pulse, controller);
#endif
    }

    void TriggerStrokeEndHaptic(float chargeRatio)
    {
#if !UNITY_EDITOR
        float amplitude = Mathf.Lerp(0.2f, 0.8f, chargeRatio);
        OVRInput.SetControllerVibration(amplitude, amplitude, controller);
        Invoke(nameof(StopHaptics), 0.1f);
#endif
        Debug.Log($"[STROKE-{hand}] Fin empuje | carga: {chargeRatio * 100f:F0}% | dist: {strokeDistance:F2}m");
    }

    void StopHaptics()
    {
#if !UNITY_EDITOR
        OVRInput.SetControllerVibration(0, 0, controller);
#endif
    }
}