using UnityEngine;

/// <summary>
/// SIMULADOR DE BRAZADA PARA EDITOR (sin Meta Quest)
///
/// Agrega este script a cualquier GameObject activo en escena.
/// Arrastra los mismos VFX que usas en StrokeDetector y SwimmingController.
///
/// CONTROLES:
///   [Mantener SPACE]  → simula mantener el gatillo (carga la brazada)
///   [Soltar SPACE]    → dispara la brazada automáticamente con la carga acumulada
///   [R]               → resetea todo al estado inicial
///
/// SECUENCIA SIMULADA:
///   1. Mantener SPACE → chargingVFX ON, SFX de carga
///   2. Soltar SPACE   → chargingVFX OFF, explosionVFX ON (breve), bubblesVFX ON
///                       SFX de brazada, "movimiento" simulado por velocidad ficticia
///   3. Durante el "movimiento" → trailVFX ON
///   4. Velocidad baja a 0       → trailVFX OFF, bubblesVFX OFF, SFX de fin
///   5. Listo para la siguiente brazada
/// </summary>
public class SwimmingTestSimulator : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // VFX (arrastra los mismos GameObjects que usas en producción)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("VFX — arrastra los mismos que en StrokeDetector / SwimmingController")]
    public GameObject chargingVFX;
    public GameObject explosionVFX;
    public GameObject bubblesVFX;
    public GameObject trailVFX;

    [Header("Parámetros de simulación")]
    [Tooltip("Tiempo máximo de carga (igual que maxChargeTime en StrokeDetector)")]
    public float maxChargeTime     = 2f;
    [Tooltip("Duración del flash de explosión en segundos")]
    public float explosionDuration = 0.3f;
    [Tooltip("Velocidad inicial ficticia al disparar la brazada (m/s simulados)")]
    public float strokeInitialSpeed = 5f;
    [Tooltip("Cuánto frena por segundo (simula el waterDrag)")]
    public float dragRate          = 2.5f;
    [Tooltip("Velocidad mínima para que el trail permanezca activo")]
    public float trailMinSpeed     = 0.1f;

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────────────────
    private enum State { Idle, Charging, Stroking, Moving }
    private State  currentState     = State.Idle;
    private float  chargeStartTime  = -1f;
    private float  chargePercent    = 0f;
    private float  simulatedSpeed   = 0f;
    private bool   chargeSFXPlayed  = false;

    // ─────────────────────────────────────────────────────────────────────────
    // UNITY LOOP
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        SetAllVFX(false);
        Debug.Log("[SIM] Simulador listo. Mantén SPACE para cargar, suelta para disparar. R = reset.");
    }

    void Update()
    {
        HandleInput();
        UpdateSimulation();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────────────────────────────────
    void HandleInput()
    {
        // ── Inicio de carga ───────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Space) && currentState == State.Idle)
        {
            EnterCharging();
        }

        // ── Disparo de brazada ────────────────────────────────────────────────
        if (Input.GetKeyUp(KeyCode.Space) && currentState == State.Charging)
        {
            FireStroke();
        }

        // ── Reset ─────────────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulator();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADOS
    // ─────────────────────────────────────────────────────────────────────────
    void EnterCharging()
    {
        currentState    = State.Charging;
        chargeStartTime = Time.time;
        chargePercent   = 0f;
        chargeSFXPlayed = false;

        SetVFX(chargingVFX, true);

        Debug.Log("[SIM] ▶ CARGANDO — mantén SPACE...");
    }

    void FireStroke()
    {
        currentState = State.Stroking;
        chargePercent = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime) * 100f;

        // Velocidad inicial proporcional a la carga
        float chargeRatio = chargePercent / 100f;
        simulatedSpeed    = strokeInitialSpeed * Mathf.Lerp(0.3f, 1f, chargeRatio);

        // ── VFX ──────────────────────────────────────────────────────────────
        SetVFX(chargingVFX,  false);
        SetVFX(explosionVFX, true);
        SetVFX(bubblesVFX,   true);
        SetVFX(trailVFX,     true);
        Invoke(nameof(HideExplosion), explosionDuration);

        // ── SFX ──────────────────────────────────────────────────────────────
        AudioManager.Instance.PlaySFX(AudioManager.Instance.brazadas, transform.position);

        Debug.Log($"[SIM] 💥 BRAZADA DISPARADA | carga: {chargePercent:F0}% | vel: {simulatedSpeed:F2}");
    }

    void UpdateSimulation()
    {
        // ── Actualizar barra de carga mientras se mantiene SPACE ──────────────
        if (currentState == State.Charging)
        {
            chargePercent = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime) * 100f;

            // SFX de carga: una sola vez al empezar
            if (!chargeSFXPlayed)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cargandoBrazada, transform.position);
                chargeSFXPlayed = true;
            }

            // Log cada 10% de carga
            if (Time.frameCount % 10 == 0)
                Debug.Log($"[SIM] ⚡ Carga: {chargePercent:F0}%");

            // Si llega al 100% avisa (no dispara automáticamente — requiere soltar SPACE)
            if (chargePercent >= 100f && Time.frameCount % 60 == 0)
                Debug.Log("[SIM] 🔴 ¡CARGA MÁXIMA! Suelta SPACE para disparar.");
        }

        // ── Simular deceleración por drag ─────────────────────────────────────
        if (currentState == State.Stroking || currentState == State.Moving)
        {
            currentState   = State.Moving;
            simulatedSpeed = Mathf.MoveTowards(simulatedSpeed, 0f, dragRate * Time.deltaTime);

            // Al llegar a velocidad mínima: fin del movimiento
            if (simulatedSpeed <= trailMinSpeed)
            {
                EndMovement();
            }
        }
    }

    void EndMovement()
    {
        currentState   = State.Idle;
        simulatedSpeed = 0f;

        // ── VFX ──────────────────────────────────────────────────────────────
        SetVFX(trailVFX,   false);
        SetVFX(bubblesVFX, false);

        Debug.Log("[SIM] ✅ Movimiento terminado. Listo para siguiente brazada (SPACE).");
    }

    void ResetSimulator()
    {
        currentState   = State.Idle;
        simulatedSpeed = 0f;
        chargePercent  = 0f;
        CancelInvoke(nameof(HideExplosion));
        SetAllVFX(false);
        Debug.Log("[SIM] 🔄 Reset.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    void HideExplosion() => SetVFX(explosionVFX, false);

    void SetVFX(GameObject vfx, bool active)
    {
        if (vfx != null) vfx.SetActive(active);
    }

    void SetAllVFX(bool active)
    {
        SetVFX(chargingVFX,  active);
        SetVFX(explosionVFX, active);
        SetVFX(bubblesVFX,   active);
        SetVFX(trailVFX,     active);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GIZMOS — barra de carga visible en Scene View
    // ─────────────────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 barOrigin = transform.position + Vector3.up * 0.3f;
        float   barWidth  = 1f;

        // Fondo gris
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(barOrigin, barOrigin + Vector3.right * barWidth);

        // Carga en cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(barOrigin, barOrigin + Vector3.right * barWidth * (chargePercent / 100f));

        // Velocidad en verde
        Gizmos.color = Color.green;
        Gizmos.DrawLine(barOrigin + Vector3.up * 0.05f,
                        barOrigin + Vector3.up * 0.05f + Vector3.right * barWidth * (simulatedSpeed / strokeInitialSpeed));
    }

#if UNITY_EDITOR
    // Panel de estado en la ventana de juego via OnGUI
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize  = 14,
            alignment = TextAnchor.MiddleLeft
        };
        style.normal.textColor = Color.white;

        string estadoEmoji = currentState switch
        {
            State.Idle     => "⬜ IDLE",
            State.Charging => $"⚡ CARGANDO {chargePercent:F0}%",
            State.Stroking => "💥 BRAZADA",
            State.Moving   => $"🫧 MOVIENDO vel: {simulatedSpeed:F2}",
            _              => "?"
        };

        string controles = "SPACE = cargar/disparar   |   R = reset";

        GUI.Box(new Rect(10, 10, 380, 60),
                $"  {estadoEmoji}\n  {controles}", style);
    }
#endif
}
