using UnityEngine;

public class MermaidAvatarController : MonoBehaviour
{
    [Header("Modelo")]
    public Transform modeloSirena;
    public Transform camara;

    [Header("IK Targets")]
    public Transform targetHandR;
    public Transform targetHandL;

    [Header("Quest Controllers")]
    public Transform rightControllerAnchor;
    public Transform leftControllerAnchor;

    [Header("Rotation Offset")]
    public Vector3 rightRotationOffset;
    public Vector3 leftRotationOffset;

    [Header("Rotación de cuerpo")]
    public SwimmingController swimmingController;
    [Tooltip("Velocidad mínima para que el cuerpo rote con la cabeza")]
    public float rotationSpeedThreshold = 0.1f;
    [Tooltip("Qué tan rápido el cuerpo sigue a la cabeza (1=instantáneo)")]
    [Range(0.01f, 1f)]
    public float bodyRotationSmoothing = 0.1f;

    void Update()
    {
        // ── Posición: el modelo sigue a la cámara ────────────────────────────
        modeloSirena.position = camara.position;

        // ── Rotación: solo cuando el jugador nada activamente ────────────────
        if (swimmingController != null && 
            swimmingController.currentSpeed > rotationSpeedThreshold)
        {
            modeloSirena.rotation = Quaternion.Slerp(
                modeloSirena.rotation,
                camara.rotation,
                bodyRotationSmoothing
            );
        }

        // ── IK manos ─────────────────────────────────────────────────────────
        if (rightControllerAnchor != null && targetHandR != null)
        {
            targetHandR.position = rightControllerAnchor.position;
            targetHandR.rotation = rightControllerAnchor.rotation *
                                   Quaternion.Euler(rightRotationOffset);
        }

        if (leftControllerAnchor != null && targetHandL != null)
        {
            targetHandL.position = leftControllerAnchor.position;
            targetHandL.rotation = leftControllerAnchor.rotation *
                                   Quaternion.Euler(leftRotationOffset);
        }
    }
}