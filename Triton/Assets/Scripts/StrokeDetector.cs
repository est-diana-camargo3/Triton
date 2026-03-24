using UnityEngine;

public class StrokeDetector : MonoBehaviour
{
    public enum HandSide { Left, Right }
    public HandSide hand;

    [Header("Umbrales")]
    public float pullVelocityThreshold = 1.2f;
    public float resetDistance = 0.3f;

    [Header("Debug")]
    public float currentVelocity;
    public string currentPhase;

    public System.Action<float> OnStrokeDetected;

    private OVRInput.Controller controller;
    private Vector3 lastPosition;
    private bool armExtended = false;

    // Teclas configurables desde el inspector
    [Header("Teclado (solo editor)")]
    public KeyCode simStrokeKey = KeyCode.Q;  // izquierda
    // La mano derecha usa otra tecla, configúrala en el inspector

    void Start()
    {
        controller = (hand == HandSide.Left)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;

        lastPosition = Vector3.zero;
    }

    void Update()
    {
#if UNITY_EDITOR
        SimulateInEditor();
#else
        DetectFromController();
#endif
    }

    void DetectFromController()
    {
        Vector3 velocity = OVRInput.GetLocalControllerVelocity(controller);
        Vector3 position = OVRInput.GetLocalControllerPosition(controller);

        currentVelocity = velocity.magnitude;

        if (!armExtended && position.z < lastPosition.z - resetDistance)
        {
            armExtended = true;
            currentPhase = "Extendido - listo para jalar";
        }

        if (armExtended && velocity.z > pullVelocityThreshold)
        {
            float power = Mathf.Clamp(velocity.magnitude, 0f, 5f);
            TriggerStroke(power);
        }
        else
        {
            currentPhase = armExtended ? "Esperando jalón..." : "Esperando extensión";
        }

        lastPosition = position;
    }

    void SimulateInEditor()
    {
        // Simula una brazada completa al presionar la tecla
        if (Input.GetKeyDown(simStrokeKey))
        {
            float simulatedPower = 2.5f;
            currentVelocity = simulatedPower;
            currentPhase = "Simulado con teclado";
            TriggerStroke(simulatedPower);
        }
        else
        {
            currentPhase = $"Editor: presiona [{simStrokeKey}] para simular";
            currentVelocity = 0f;
        }
    }

    void TriggerStroke(float power)
    {
        OnStrokeDetected?.Invoke(power);
        armExtended = false;
        currentPhase = $"BRAZADA detectada | power: {power:F2}";

#if !UNITY_EDITOR
    // Vibración proporcional a la fuerza de la brazada
    float hapticAmplitude = Mathf.Clamp01(power / 5f);  // normaliza entre 0 y 1
    float hapticDuration  = 0.1f;

    OVRInput.SetControllerVibration(hapticAmplitude, hapticAmplitude, controller);
    Invoke(nameof(StopHaptics), hapticDuration);
#endif

        Debug.Log($"[{hand}] Brazada! Power: {power:F2}");
    }

    void StopHaptics()
    {
#if !UNITY_EDITOR
    OVRInput.SetControllerVibration(0, 0, controller);
#endif
    }
}
