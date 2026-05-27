using UnityEngine;

public class WhaleController : MonoBehaviour
{
    [Header("Estado visual — activar cuando esté lista")]
    public GameObject particulasActiva;
    public GameObject luzProtagonismo;

    [Header("Zona de trigger")]
    public float radioDeteccion = 2f;

    [Header("Diálogo — opcional")]
    public DialogueSO dialogo;

    private Transform _jugadorTransform;
    private bool _estaActiva = false;
    private bool _yaDisparoFinal = false;

    // ── Suscripción al patrón Observer ─────────────────────────────

    private void OnEnable()
    {
        GameFlowManager.OnEstadoCambio += ManejarCambioEstado;
    }

    private void OnDisable()
    {
        GameFlowManager.OnEstadoCambio -= ManejarCambioEstado;
    }

    // ── Inicialización ──────────────────────────────────────────────

    private void Start()
    {
        _jugadorTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Estado inicial: ballena inactiva
        SetActivaVisual(false);
    }

    // ── Observer: reaccionar a cambios de estado del juego ──────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.WhaleActive:
                ActivarBallena();
                break;

            case GameState.Resolving:
            case GameState.LoadingEnding:
                // Desactivar efectos visuales al salir del gameplay
                SetActivaVisual(false);
                break;
        }
    }

    // ── Lógica de activación ────────────────────────────────────────

    private void ActivarBallena()
    {
        _estaActiva = true;
        _yaDisparoFinal = false;
        SetActivaVisual(true);

        Debug.Log("[WHALE] Ballena activa — esperando al jugador");
    }

    private void SetActivaVisual(bool activo)
    {
        if (particulasActiva != null) particulasActiva.SetActive(activo);
        if (luzProtagonismo != null)  luzProtagonismo.SetActive(activo);
    }

    // ── Detección del jugador ───────────────────────────────────────

    private void Update()
    {
        if (!_estaActiva || _yaDisparoFinal) return;
        if (_jugadorTransform == null) return;

        float distancia = Vector3.Distance(transform.position, _jugadorTransform.position);

        if (distancia <= radioDeteccion)
            OnJugadorLlego();
    }

    private void OnJugadorLlego()
    {
        _yaDisparoFinal = true;
        _estaActiva = false;

        Debug.Log("[WHALE] Jugador llegó — disparando final");

        // Diálogo opcional antes de la transición
        if (dialogo != null)
            DialogueManager.Instance.ReproducirPorProximidad(dialogo);

        GameFlowManager.Instance.OnJugadorLlegoABallena();
    }

    // ── Gizmo de ayuda en editor ────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}