using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Canvas del menú")]
    public GameObject panelMenu;

    [Header("Selector de modo de locomoción")]
    public Button botonBrazadas;
    public Button botonJoystick;

    [Header("Colores del selector")]
    public Color colorActivo   = new Color(0.20f, 0.60f, 1.00f, 1f); // azul
    public Color colorInactivo = new Color(0.25f, 0.25f, 0.25f, 1f); // gris

    [Header("Navegación")]
    public MenuNavigator navegador;
    
    // ── Observer ────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameFlowManager.OnEstadoCambio       += ManejarCambioEstado;
        LocomotionManager.OnModoChanged      += ActualizarVisuales;
    }

    private void OnDisable()
    {
        GameFlowManager.OnEstadoCambio       -= ManejarCambioEstado;
        LocomotionManager.OnModoChanged      -= ActualizarVisuales;
    }

    // ── Inicialización ──────────────────────────────────────────────

    private void Start()
    {
        MostrarMenu(true);

        // Reflejar preferencia guardada en los botones
        LocomotionMode modoGuardado = LocomotionManager.Instance?.ModoActual
                                      ?? LocomotionMode.Brazadas;
        ActualizarVisuales(modoGuardado);
    }

    // ── Observer ────────────────────────────────────────────────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.Menu:
                MostrarMenu(true);
                break;

            case GameState.CinematicIntro:
            case GameState.LoadingGameplay:
            case GameState.CinematicEnding:
                MostrarMenu(false);
                break;
        }
    }

    // ── Botones de gameplay ─────────────────────────────────────────

    public void OnBotonJugar()
    {
        Debug.Log("[MENU] Jugar");
        GameFlowManager.Instance.IniciarJuego();
    }

    public void OnBotonSalir()
    {
        Debug.Log("[MENU] Salir");
        GameFlowManager.Instance.SalirJuego();
    }

    // ── Selector de locomoción ──────────────────────────────────────

    public void OnBotonSeleccionarBrazadas()
    {
        LocomotionManager.Instance.SetModo(LocomotionMode.Brazadas);
    }

    public void OnBotonSeleccionarJoystick()
    {
        LocomotionManager.Instance.SetModo(LocomotionMode.Joystick);
    }

    // ── Visual del selector ─────────────────────────────────────────

    private void ActualizarVisuales(LocomotionMode modo)
    {
        if (botonBrazadas != null)
            botonBrazadas.GetComponent<Image>().color =
                modo == LocomotionMode.Brazadas ? colorActivo : colorInactivo;

        if (botonJoystick != null)
            botonJoystick.GetComponent<Image>().color =
                modo == LocomotionMode.Joystick ? colorActivo : colorInactivo;
    }

    // ── Utilidad ────────────────────────────────────────────────────

    private void MostrarMenu(bool mostrar)
    {
        if (panelMenu != null) panelMenu.SetActive(mostrar);

        // Resetear selección cada vez que el menú aparece
        if (mostrar && navegador != null)
            navegador.ResetSeleccion();
    }
}