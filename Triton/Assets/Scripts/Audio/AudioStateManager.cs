using UnityEngine;

public class AudioStateManager : MonoBehaviour
{
    [Header("Ambiente por estado")]
    public SoundData ambienteMenu;
    public SoundData ambienteGameplay;

    [Header("Música adaptativa — buscar en Pixabay o Uppbeat")]
    [Tooltip("0-1 cartas colocadas — tranquila, exploratoria")]
    public SoundData musicaCalmada;

    [Tooltip("2 cartas colocadas — más tensa, expectante")]
    public SoundData musicaTension;

    [Tooltip("3 cartas / ballena activa — resolución inminente")]
    public SoundData musicaResolucion;

    [Header("Transiciones")]
    public float duracionFade = 2f;

    private int _cartasAnteriores = -1;

    // ── Observer ────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameFlowManager.OnEstadoCambio        += ManejarCambioEstado;
        PlacementStateManager.OnCartaColocada += ManejarCartaColocada;
    }

    private void OnDisable()
    {
        GameFlowManager.OnEstadoCambio        -= ManejarCambioEstado;
        PlacementStateManager.OnCartaColocada -= ManejarCartaColocada;
    }

    // ── Reaccionar a GameState ───────────────────────────────────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.Menu:
                // Ambiente suave para el menú, sin música de gameplay
                AudioManager.Instance.PlayAmbience(ambienteMenu);
                AudioManager.Instance.FadeToMusic(null, duracionFade);
                break;

            case GameState.Gameplay:
                // Ambiente submarino + música inicial calmada
                AudioManager.Instance.PlayAmbience(ambienteGameplay);
                AudioManager.Instance.FadeToMusic(musicaCalmada, duracionFade);
                _cartasAnteriores = 0;
                break;

            case GameState.WhaleActive:
                // La ballena está lista — música de resolución
                AudioManager.Instance.FadeToMusic(musicaResolucion, duracionFade);
                break;

            case GameState.CinematicIntro:
            case GameState.CinematicEnding:
                // Las cinemáticas tienen su propio audio en el video
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.StopAmbience();
                break;

            case GameState.LoadingGameplay:
            case GameState.LoadingEnding:
                // Fade out suave durante la carga
                AudioManager.Instance.FadeToMusic(null, duracionFade);
                break;
        }
    }

    // ── Reaccionar a cartas colocadas ───────────────────────────────

    private void ManejarCartaColocada(int totalCartas)
    {
        // Solo durante gameplay activo
        if (GameFlowManager.Instance.EstadoActual != GameState.Gameplay) return;

        // Ignorar si el número no cambió (puede dispararse al retirar también)
        if (totalCartas == _cartasAnteriores) return;
        _cartasAnteriores = totalCartas;

        Debug.Log($"[AUDIO] Cartas colocadas: {totalCartas} — ajustando música");

        switch (totalCartas)
        {
            case 1:
                // Primera carta — todavía calmado, sin cambio
                break;

            case 2:
                // Segunda carta — tensión creciente
                AudioManager.Instance.FadeToMusic(musicaTension, duracionFade);
                break;

            case 0:
                // Jugador retiró todas las cartas — volver a calmado
                AudioManager.Instance.FadeToMusic(musicaCalmada, duracionFade);
                break;
        }
    }
}