using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    public DialogueSO dialogo;

    [Header("Cooldown entre activaciones")]
    public float cooldownSegundos = 8f;

    [Header("¿Actualizar diálogo cuando cambian las cartas?")]
    [Tooltip("Activar solo en la ballena")]
    public bool escucharCambioDeCartas = false;

    private float _tiempoUltimaActivacion = -999f;
    private bool _jugadorDentro = false;

    // ── Observer — solo si está habilitado ──────────────────────────

    private void OnEnable()
    {
        if (escucharCambioDeCartas)
            PlacementStateManager.OnCartaColocada += OnCartaCambio;
    }

    private void OnDisable()
    {
        if (escucharCambioDeCartas)
            PlacementStateManager.OnCartaColocada -= OnCartaCambio;
    }

    // ── Trigger de proximidad — con guard de re-entrada ─────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_jugadorDentro) return; // ya estaba adentro, ignorar re-entrada

        _jugadorDentro = true;

        if (Time.time - _tiempoUltimaActivacion < cooldownSegundos) return;
        _tiempoUltimaActivacion = Time.time;

        DialogueManager.Instance.ReproducirPorProximidad(dialogo);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _jugadorDentro = false; // reset — puede dispararse al volver a entrar
    }

    // ── Reaccionar a cambio de cartas si el jugador ya está cerca ───

    private void OnCartaCambio(int totalCartas)
    {
        if (!_jugadorDentro) return; // solo si el jugador ya está en zona

        // Respetar cooldown también aquí
        if (Time.time - _tiempoUltimaActivacion < cooldownSegundos) return;
        _tiempoUltimaActivacion = Time.time;

        DialogueManager.Instance.ReproducirPorProximidad(dialogo);

        Debug.Log($"[NPC] Diálogo actualizado por cambio de cartas: {totalCartas}/3");
    }
}