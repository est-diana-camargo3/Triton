using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("AudioSource dedicado al diálogo")]
    public AudioSource audioSource;

    // Rastrea qué líneas contextuales ya fueron reproducidas
    // Clave: (DialogueSO, cartasColocadas) → ya reproducida
    private HashSet<(DialogueSO, int)> _contextualesReproducidos = new();

    private bool _estaHablando = false;

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

    // ── API pública ─────────────────────────────────────────────────

    // Llamado por NPCs al detectar proximidad del jugador
    public void ReproducirPorProximidad(DialogueSO dialogo)
    {
        if (dialogo == null || _estaHablando) return;

        int cartasActuales = PlacementStateManager.Instance.CartasColocadas();
        var clave = (dialogo, cartasActuales);

        // Si el contextual de este momento no se ha dicho, lo decimos
        if (!_contextualesReproducidos.Contains(clave))
        {
            AudioClip contextual = dialogo.ObtenerContextual(cartasActuales);
            if (contextual != null)
            {
                _contextualesReproducidos.Add(clave);
                StartCoroutine(Reproducir(contextual));
                return;
            }
        }

        // Si ya se dijo el contextual, intentamos una línea random
        AudioClip random = dialogo.ObtenerRandom();
        if (random != null)
            StartCoroutine(Reproducir(random));
    }

    // Llamado por ChestController al entrar al trigger
    public void ReproducirEntradaCofre(DialogueSO dialogo)
    {
        if (dialogo == null || _estaHablando) return;
        if (dialogo.lineaAlEntrarTrigger == null) return;

        StartCoroutine(Reproducir(dialogo.lineaAlEntrarTrigger));
    }

    // Llamado por ChestController al cerrar con carta
    public void ReproducirCierreCofre(DialogueSO dialogo)
    {
        if (dialogo == null || _estaHablando) return;
        if (dialogo.lineaAlCerrarConCarta == null) return;

        StartCoroutine(Reproducir(dialogo.lineaAlCerrarConCarta));
    }

    // ── Reproducción ────────────────────────────────────────────────

    private IEnumerator Reproducir(AudioClip clip)
    {
        _estaHablando = true;
        audioSource.PlayOneShot(clip);

        Debug.Log($"[DIALOGUE] Reproduciendo: {clip.name}");

        yield return new WaitForSeconds(clip.length);
        _estaHablando = false;
    }
}