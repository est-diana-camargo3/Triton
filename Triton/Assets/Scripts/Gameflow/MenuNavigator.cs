using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuNavigator : MonoBehaviour
{
    [System.Serializable]
    public class MenuItem
    {
        public string label;
        public Button button;           // referencia al Button de Unity
        public System.Action accion;    // qué ejecutar al confirmar
    }

    [Header("Ítems navegables — en orden de arriba a abajo")]
    public MenuItem[] items;

    [Header("Visual del ítem seleccionado")]
    public Color colorSeleccionado = new Color(0.00f, 0.70f, 0.78f, 1f); // cyan
    public Color colorNormal       = new Color(0.25f, 0.25f, 0.25f, 1f); // gris

    [Header("Navegación")]
    [Tooltip("Tiempo mínimo entre cambios de ítem para evitar scroll descontrolado")]
    public float cooldownNavegacion = 0.25f;

    [Tooltip("Umbral del thumbstick para considerar intención de navegación")]
    public float umbralThumbstick = 0.5f;

    // ── Estado interno ───────────────────────────────────────────────
    private int _indiceActual = 0;
    private float _timerNavegacion = 0f;
    private bool _thumbstickEnUso = false;

    // ── Inicialización ───────────────────────────────────────────────

    private void Start()
    {
        ActualizarVisual();
    }

    // ── Input loop ───────────────────────────────────────────────────

    private void Update()
    {
        if (items == null || items.Length == 0) return;

        _timerNavegacion -= Time.deltaTime;

        ManejarNavegacion();
        ManejarConfirmacion();
    }

    // ── Navegación con cualquier thumbstick ──────────────────────────

    private void ManejarNavegacion()
    {
        // Leer ambos thumbsticks y tomar el de mayor magnitud en Y
        float ejeY_izq = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        float ejeY_der = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        float ejeY = Mathf.Abs(ejeY_izq) > Mathf.Abs(ejeY_der) ? ejeY_izq : ejeY_der;

        bool intentaNavegarArriba = ejeY >  umbralThumbstick;
        bool intentaNavegarAbajo  = ejeY < -umbralThumbstick;
        bool hayIntencion = intentaNavegarArriba || intentaNavegarAbajo;

        // Debounce: solo navegar si pasó el cooldown y el stick acaba de moverse
        if (hayIntencion && !_thumbstickEnUso && _timerNavegacion <= 0f)
        {
            _thumbstickEnUso = true;
            _timerNavegacion = cooldownNavegacion;

            if (intentaNavegarArriba)
                Navegar(-1); // arriba = índice menor
            else
                Navegar(+1); // abajo = índice mayor
        }

        // Reset cuando el stick vuelve al centro
        if (!hayIntencion)
            _thumbstickEnUso = false;
    }

    private void Navegar(int direccion)
    {
        _indiceActual = (_indiceActual + direccion + items.Length) % items.Length;
        ActualizarVisual();

        Debug.Log($"[MENU] Seleccionado: {items[_indiceActual].label}");
    }

    // ── Confirmación con botones o triggers ──────────────────────────

    private void ManejarConfirmacion()
    {
        bool confirmar =
            OVRInput.GetDown(OVRInput.Button.One,  OVRInput.Controller.RTouch) || // A
            OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.LTouch) || // X
            OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ||
            OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

        if (confirmar)
            ConfirmarSeleccion();
    }

    private void ConfirmarSeleccion()
    {
        if (_indiceActual < 0 || _indiceActual >= items.Length) return;

        Debug.Log($"[MENU] Confirmado: {items[_indiceActual].label}");

        // Invocar el Button de Unity (respeta OnClick del Inspector)
        items[_indiceActual].button?.onClick.Invoke();
    }

    // ── Visual ───────────────────────────────────────────────────────

    private void ActualizarVisual()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].button == null) continue;

            Image img = items[i].button.GetComponent<Image>();
            if (img == null) continue;

            img.color = (i == _indiceActual) ? colorSeleccionado : colorNormal;
        }
    }

    // ── API pública — para activar/desactivar desde MenuController ───

    public void ResetSeleccion()
    {
        _indiceActual = 0;
        _timerNavegacion = 0f;
        _thumbstickEnUso = false;
        ActualizarVisual();
    }
}