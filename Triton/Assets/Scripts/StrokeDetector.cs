using UnityEngine;

public class StrokeDetector : MonoBehaviour
{
    public enum HandSide { Left, Right }
    public HandSide hand;

    [Header("Umbrales")]
    public float velocityThreshold = 1.2f; // un poco más alto para reducir falsos positivos

    [Header("Cooldown")]
    public float strokeCooldown = 0.5f;
    private float lastStrokeTime;

    [Header("Debug")]
    public float currentVelocity;
    public Vector3 currentRawVelocity;
    public string currentPhase;

    public System.Action<float, Vector3> OnStrokeDetected;

    private OVRInput.Controller controller;

    void Start()
    {
        controller = (hand == HandSide.Left)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;
    }

    void Update()
    {
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        if (trigger < 0.8f)
        {
            currentPhase = "Gatillo no presionado";
            return;
        }

        Vector3 velocity = OVRInput.GetLocalControllerVelocity(controller);
        currentRawVelocity = velocity;
        currentVelocity = velocity.magnitude;

        bool cooldownOk = Time.time - lastStrokeTime > strokeCooldown;

        if (currentVelocity > velocityThreshold && cooldownOk)
        {
            Vector3 strokeDirection = -velocity.normalized;
            lastStrokeTime = Time.time;
            currentPhase = "Brazada!";
            TriggerStroke(currentVelocity, strokeDirection);
        }
        else
        {
            currentPhase = cooldownOk 
                ? "Esperando movimiento" 
                : $"Cooldown ({(strokeCooldown - (Time.time - lastStrokeTime)):F1}s)";
        }

        if (Time.frameCount % 30 == 0)
            Debug.Log($"[STROKE-{hand}] vel: {currentVelocity:F3} | fase: {currentPhase}");
    }

    void TriggerStroke(float power, Vector3 direction)
    {
        OnStrokeDetected?.Invoke(power, direction);
        Debug.Log($"[STROKE-{hand}] BRAZADA | power: {power:F2} | dir: {direction}");

#if !UNITY_EDITOR
        float hapticAmplitude = Mathf.Clamp01(power / 5f);
        OVRInput.SetControllerVibration(hapticAmplitude, hapticAmplitude, controller);
        Invoke(nameof(StopHaptics), 0.1f);
#endif
    }

    void StopHaptics()
    {
#if !UNITY_EDITOR
        OVRInput.SetControllerVibration(0, 0, controller);
#endif
    }
}