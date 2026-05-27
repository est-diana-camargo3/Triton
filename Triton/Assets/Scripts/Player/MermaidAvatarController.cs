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
    public float rotationSpeedThreshold = 0.1f;
    [Tooltip("Velocidad base de rotación en grados por segundo")]
    public float baseRotationSpeed = 90f;
    [Tooltip("Ángulo a partir del cual la velocidad se multiplica")]
    public float maxBodyHeadAngle = 60f;

    void Update()
    {
        modeloSirena.position = camara.position;

      
        float angle = Vector3.Angle(modeloSirena.forward, camara.forward);
        bool isSwimming = swimmingController != null &&
                          swimmingController.currentSpeed > rotationSpeedThreshold;

// Velocidad escala con el ángulo: cerca de 0° rota lento, cerca de 180° rota rápido
        float angleNormalized = Mathf.Clamp01(angle / 180f);
        float rotationSpeed   = baseRotationSpeed * angleNormalized;

// Cuando nada, velocidad mínima garantizada para que el cuerpo siempre siga
        if (isSwimming)
            rotationSpeed = Mathf.Max(rotationSpeed, baseRotationSpeed * 0.5f);

        modeloSirena.rotation = Quaternion.RotateTowards(
            modeloSirena.rotation,
            camara.rotation,
            rotationSpeed * Time.deltaTime
        );

        // ── IK manos (sin cambios) ────────────────────────────────────────────
        if (rightControllerAnchor != null && targetHandR != null)
        {
            // rightControllerAnchor.rotation.
                
            
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