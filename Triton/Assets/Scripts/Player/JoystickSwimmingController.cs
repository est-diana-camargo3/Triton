using UnityEngine;

public class JoystickSwimmingController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Para leer strokeForceMultiplier, maxSpeed y waterDrag compartidos")]
    public SwimmingController swimmingController;
    public Transform centerEye;

    private Rigidbody _rb;
    private bool _activo;

    // ── Observer ────────────────────────────────────────────────────

    private void OnEnable()
    {
        LocomotionManager.OnModoChanged += ManejarCambioModo;
    }

    private void OnDisable()
    {
        LocomotionManager.OnModoChanged -= ManejarCambioModo;
    }

    // ── Inicialización ──────────────────────────────────────────────

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // Estado inicial desde preferencia guardada
        _activo = LocomotionManager.Instance?.ModoActual == LocomotionMode.Joystick;
    }

    private void ManejarCambioModo(LocomotionMode modo)
    {
        _activo = modo == LocomotionMode.Joystick;
    }

    // ── Movimiento ──────────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (!_activo || swimmingController == null || centerEye == null) return;

        Vector2 joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Dead zone
        if (joystick.magnitude < 0.1f) return;

        // Dirección 3D completa relativa a la cámara
        // El jugador mira hacia arriba → se mueve hacia arriba (igual que brazadas)
        Vector3 moveDirection = (centerEye.forward * joystick.y +
                                 centerEye.right   * joystick.x).normalized;

        // Misma fuerza y velocidad máxima que el sistema de brazadas
        float fuerza = joystick.magnitude * swimmingController.strokeForceMultiplier;
        _rb.AddForce(moveDirection * fuerza, ForceMode.Force);

        if (_rb.velocity.magnitude > swimmingController.maxSpeed)
            _rb.velocity = _rb.velocity.normalized * swimmingController.maxSpeed;

        Debug.Log($"[JOYSTICK] Dir: {moveDirection:F2} | F: {fuerza:F2}");
    }
}
