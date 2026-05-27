using UnityEngine;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Canvas de carga")]
    public GameObject panelCarga;

    [Header("Shader de olas — referencia al Renderer del plano")]
    public Renderer planoAgua;

    [Header("Animación del shader")]
    public float velocidadBase   = 1f;
    public float velocidadMaxima = 3f; // acelera al final de la carga
    public string parametroVelocidad = "_Speed"; // nombre del param en el shader

    [Header("Texto opcional")]
    public TMPro.TextMeshProUGUI textoCargando;
    private string[] _mensajesCarga = { "Descendiendo...", "El mar te espera...", "Preparando las profundidades..." };

    private Coroutine _animacionActiva;

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
        // Empieza oculto
        MostrarCarga(false);
    }

    // ── Observer ────────────────────────────────────────────────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.LoadingGameplay:
            case GameState.LoadingEnding:
                MostrarCarga(true);
                if (_animacionActiva != null) StopCoroutine(_animacionActiva);
                _animacionActiva = StartCoroutine(AnimarOlas());
                break;

            case GameState.Gameplay:
            case GameState.CinematicEnding:
                // La carga terminó — detener animación y ocultar
                if (_animacionActiva != null)
                {
                    StopCoroutine(_animacionActiva);
                    _animacionActiva = null;
                }
                MostrarCarga(false);
                break;
        }
    }

    // ── Animación del shader ────────────────────────────────────────

    private IEnumerator AnimarOlas()
    {
        float velocidadActual = velocidadBase;

        if (planoAgua != null)
            planoAgua.material.SetFloat(parametroVelocidad, velocidadActual);

        // Texto de carga aleatorio
        if (textoCargando != null)
            textoCargando.text = _mensajesCarga[Random.Range(0, _mensajesCarga.Length)];

        // Acelerar las olas gradualmente — da sensación de descenso
        float tiempo = 0f;
        float duracionAceleracion = 1.5f;

        while (true)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracionAceleracion);
            velocidadActual = Mathf.Lerp(velocidadBase, velocidadMaxima, t);

            if (planoAgua != null)
                planoAgua.material.SetFloat(parametroVelocidad, velocidadActual);

            yield return null;
        }
    }

    // ── Utilidad ────────────────────────────────────────────────────

    private void MostrarCarga(bool mostrar)
    {
        if (panelCarga != null)
            panelCarga.SetActive(mostrar);
    }
}