using UnityEngine;

/// <summary>
/// Interacción con el cofre: proximidad + botón → activa animación de apertura.
/// No interfiere con el sistema de grab ni con StrokeDetector/SwimmingController.
///
/// SETUP EN UNITY:
/// 1. Asegúrat"e de que el Animator del cofre tenga un Trigger llamado Open"
///    (o el nombre que pongas en animationTrigger)
/// 2. Agrega un Collider al cofre con "Is Trigger" activado — este define
///    el radio de interacción (hazlo un poco más grande que el cofre visual)
/// 3. Agrega este script al mismo GameObject del cofre
/// 4. El GameObject del Player debe tener el tag "Player"
///
/// BOTÓN POR DEFECTO: Joystick derecho presionado (PrimaryThumbstick button)
/// Puedes cambiarlo en el Inspector a cualquier otro botón OVR.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ChestInteraction : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Nombre exacto del Trigger en el Animator Controller del cofre")]
    public string animationTrigger = "Open";

    [Header("Interacción")]
    [Tooltip("Botón que abre el cofre cuando el jugador está cerca")]
    public OVRInput.Button openButton = OVRInput.Button.PrimaryThumbstick;
    [Tooltip("Mensaje en pantalla cuando el jugador entra al radio (opcional - para UI)")]
    public string proximityHint = "Presiona el joystick para abrir";

    [Header("Feedback")]
    [Tooltip("Sonido al abrir el cofre (opcional)")]
    public AudioSource openSound;
    [Tooltip("Vibración al abrir")]
    public bool hapticFeedback = true;

    [Header("Debug")]
    public bool playerInRange = false;
    public bool hasBeenOpened = false;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Validar que el trigger exista
        bool triggerFound = false;
        foreach (var param in animator.parameters)
        {
            if (param.name == animationTrigger && param.type == AnimatorControllerParameterType.Trigger)
            {
                triggerFound = true;
                break;
            }
        }
        if (!triggerFound)
            Debug.LogWarning($"[COFRE] No se encontró un Trigger llamado '{animationTrigger}' en el Animator. " +
                             "Revisa el nombre en el Inspector.");
    }

    void Update()
    {
        if (!playerInRange || hasBeenOpened) return;

        bool buttonPressed = OVRInput.GetDown(openButton);

#if UNITY_EDITOR
        // En editor: E para abrir
        buttonPressed |= Input.GetKeyDown(KeyCode.E);
#endif

        if (buttonPressed)
            OpenChest();
    }

    void OpenChest()
    {
        hasBeenOpened = true;
        animator.SetTrigger(animationTrigger);

        if (openSound != null)
            openSound.Play();

        if (hapticFeedback)
        {
#if !UNITY_EDITOR
            // Pulso corto en ambos controllers como confirmación
            OVRInput.SetControllerVibration(0.6f, 0.6f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0.6f, 0.6f, OVRInput.Controller.RTouch);
            Invoke(nameof(StopHaptics), 0.2f);
#endif
        }

        Debug.Log("[COFRE] Abierto");
    }

    void StopHaptics()
    {
#if !UNITY_EDITOR
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
#endif
    }

    // ── Trigger de proximidad ─────────────────────────────────────────────────
    // El Collider del cofre con Is Trigger = true detecta al Player
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        Debug.Log($"[COFRE] Jugador en rango - {proximityHint}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        Debug.Log("[COFRE] Jugador fuera de rango");
    }

    // Dibuja el radio de interacción en Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        var col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
