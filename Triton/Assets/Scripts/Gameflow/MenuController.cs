using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Canvas del menú")]
    public GameObject panelMenu;

    // ── Observer ────────────────────────────────────────────────────

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
        // La escena arranca en menú
        MostrarMenu(true);
    }

    // ── Observer ────────────────────────────────────────────────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.Menu:
                MostrarMenu(true);
                break;

            // En cualquier otro estado el menú desaparece
            case GameState.CinematicIntro:
            case GameState.LoadingGameplay:
            case GameState.CinematicEnding:
                MostrarMenu(false);
                break;
        }
    }

    // ── API de botones ──────────────────────────────────────────────

    // Asignar al botón "Jugar" en el Inspector (OnClick)
    public void OnBotonJugar()
    {
        Debug.Log("[MENU] Jugar presionado");
        GameFlowManager.Instance.IniciarJuego();
    }

    // Asignar al botón "Salir" en el Inspector (OnClick)
    public void OnBotonSalir()
    {
        Debug.Log("[MENU] Salir presionado");
        GameFlowManager.Instance.SalirJuego();
    }

    // ── Utilidad ────────────────────────────────────────────────────

    private void MostrarMenu(bool mostrar)
    {
        if (panelMenu != null)
            panelMenu.SetActive(mostrar);
    }
}