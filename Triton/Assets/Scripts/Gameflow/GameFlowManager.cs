using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    // ── Evento central — todos los sistemas escuchan esto ──────────
    public static event Action<GameState> OnEstadoCambio;

    // ── Estado actual ───────────────────────────────────────────────
    public GameState EstadoActual { get; private set; } = GameState.Menu;

    // ── Nombres de escenas ──────────────────────────────────────────
    private const string ESCENA_CINEMATICA = "Cinematica";
    private const string ESCENA_GAMEPLAY   = "Gameplay";

    // ── Resultado del ending, necesario al volver a escena Cinematica
    public EndingType EndingResuelto { get; private set; } = EndingType.NingunFinal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── API pública — otros sistemas llaman estos métodos ───────────

    // Llamado por el botón Play del menú
    public void IniciarJuego()
    {
        CambiarEstado(GameState.CinematicIntro);
    }

    // Llamado por CinematicController cuando el video de intro termina
    public void OnIntroTerminada()
    {
        CambiarEstado(GameState.LoadingGameplay);
    }

    // Llamado por LoadingScreenController cuando la carga de Gameplay terminó
    public void OnGameplayCargado()
    {
        CambiarEstado(GameState.Gameplay);
    }

    // Llamado por PlacementStateManager cuando los 3 cofres están llenos
    public void OnTodosCofresLlenos()
    {
        if (EstadoActual != GameState.Gameplay) return;
        CambiarEstado(GameState.WhaleActive);
    }

    // Llamado por WhaleController cuando el jugador llega a la ballena
    public void OnJugadorLlegoABallena()
    {
        if (EstadoActual != GameState.WhaleActive) return;
        CambiarEstado(GameState.Resolving);
    }

    // Llamado por CinematicController cuando el video de ending termina
    public void OnEndingTerminado()
    {
        CambiarEstado(GameState.Menu);
    }

    // Llamado por el botón Salir del menú
    public void SalirJuego()
    {
        Application.Quit();
    }

    // ── Máquina de estados ──────────────────────────────────────────

    private void CambiarEstado(GameState nuevoEstado)
    {
        EstadoActual = nuevoEstado;
        Debug.Log($"[FLOW] Estado: {nuevoEstado}");

        // Notificar a todos los suscriptores
        OnEstadoCambio?.Invoke(nuevoEstado);

        // Lógica de transición propia del manager
        switch (nuevoEstado)
        {
            case GameState.LoadingGameplay:
                StartCoroutine(CargarEscenaAsync(ESCENA_GAMEPLAY));
                break;

            case GameState.Resolving:
                ResolverFinal();
                break;

            case GameState.LoadingEnding:
                StartCoroutine(CargarEscenaAsync(ESCENA_CINEMATICA));
                break;
        }
    }

    private void ResolverFinal()
    {
        EndingResuelto = PlacementStateManager.Instance.ResolverFinal();
        Debug.Log($"[FLOW] Final resuelto: {EndingResuelto}");
        CambiarEstado(GameState.LoadingEnding);
    }

    // ── Carga asíncrona ─────────────────────────────────────────────

    private IEnumerator CargarEscenaAsync(string nombreEscena)
    {
        // Notificar a LoadingScreenController que muestre la pantalla
        OnEstadoCambio?.Invoke(GameState.LoadingGameplay); // LoadingScreen escucha esto

        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscena);
        operacion.allowSceneActivation = false;

        // Esperar a que la carga llegue al 90% (punto de pausa de Unity)
        while (operacion.progress < 0.9f)
        {
            Debug.Log($"[FLOW] Cargando {nombreEscena}: {operacion.progress * 100f:F0}%");
            yield return null;
        }

        // Esperar mínimo 2 segundos para que la animación de olas se vea
        yield return new WaitForSeconds(2f);

        // Activar la escena
        operacion.allowSceneActivation = true;

        // Esperar un frame para que la escena termine de activarse
        yield return null;
        yield return null;
        
        // Notificar que la carga terminó según la escena destino
        if (nombreEscena == ESCENA_GAMEPLAY)
            OnGameplayCargado();
        else if (nombreEscena == ESCENA_CINEMATICA)
            CambiarEstado(GameState.CinematicEnding);
    }
}
