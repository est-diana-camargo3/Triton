using UnityEngine;
using Oculus.Interaction;

public class ChestController : MonoBehaviour
{
    [Header("Configuración")]
    public int indiceCofre;           // 0, 1 o 2 — asignar en Inspector
    public CartaID cartaEsperada;     // qué carta es correcta para este cofre

    [Header("Referencias")]
    public Transform snapPoint;       // punto donde la carta se coloca dentro del cofre
    public Animator animator;         // Animator del cofre con params "Abrir" y "Cerrar"
    public Grabbable grabbableEnZona; // referencia al Grabbable de la carta cuando entra

    [Header("Zona de interacción")]
    public float radioInteraccion = 1.2f; // radio del trigger sphere

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoAbrir;
    public AudioClip sonidoCerrar;
    public AudioClip sonidoCartaColocada;
    public AudioClip sonidoCartaRetirada;
    public AudioClip sonidoCoffeOcupado;   // feedback si intenta abrir cofre ya cerrado con carta

    [Header("Diálogo")]
    public DialogueSO dialogoCofre; // asignar Dialogo_Cofres en Inspector
    
    
    // Estado interno
    private enum EstadoCofre { CerradoVacio, EnTransicion, AbiertoVacio, AbiertoOcupado, CerradoOcupado }
    private EstadoCofre _estado = EstadoCofre.CerradoVacio;

    private bool _jugadorEnZona = false;
    private CartaSO _cartaActual = null;
    private GameObject _cartaObjActual = null;

    private Transform _jugadorTransform;

    private void Start()
    {
        // Registrarse en PlacementStateManager
        PlacementStateManager.Instance.RegistrarCofre(indiceCofre, cartaEsperada);

        // Obtener referencia al jugador por tag
        _jugadorTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // Verificar proximidad manualmente (alternativa a OnTriggerEnter para mayor control)
        float distancia = Vector3.Distance(transform.position, _jugadorTransform.position);
        bool enZonaAhora = distancia <= radioInteraccion;

        // Detectar entrada y salida de zona
        if (enZonaAhora && !_jugadorEnZona)
            OnJugadorEntroEnZona();
        else if (!enZonaAhora && _jugadorEnZona)
            OnJugadorSalioDeZona();

        _jugadorEnZona = enZonaAhora;

        // Leer joystick izquierdo solo si el jugador está en zona
        if (!_jugadorEnZona) return;
        if (_estado == EstadoCofre.EnTransicion) return; 

        float ejeY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        if (ejeY > 0.7f)
            IntentarAbrir();
        else if (ejeY < -0.7f)
            IntentarCerrar();
    }

    private void OnJugadorEntroEnZona()
    {
        DialogueManager.Instance.ReproducirEntradaCofre(dialogoCofre);
        Debug.Log($"[CHEST] Jugador en zona del cofre {indiceCofre}");
        // Aquí después: activar highlight visual del cofre
    }

    private void OnJugadorSalioDeZona()
    {
        Debug.Log($"[CHEST] Jugador salió de zona del cofre {indiceCofre}");
        // Si el cofre quedó abierto sin carta al salir, lo cerramos automáticamente
        if (_estado == EstadoCofre.AbiertoVacio)
            EjecutarCerrar();
    }

    // ── Lógica de apertura ──────────────────────────────────────────

    private void IntentarAbrir()
    {
        if (_estado == EstadoCofre.CerradoVacio)
            EjecutarAbrir();
        else if (_estado == EstadoCofre.CerradoOcupado)
            EjecutarAbrirConCarta();
        // Si ya está abierto, ignorar
    }

    private void IntentarCerrar()
    {
        if (_estado == EstadoCofre.AbiertoVacio)
            EjecutarCerrar();
        else if (_estado == EstadoCofre.AbiertoOcupado)
            EjecutarCerrarConCarta();
        // Si ya está cerrado, ignorar
    }

    private void EjecutarAbrir()
    {
        _estado = EstadoCofre.EnTransicion;
        animator.SetTrigger("Abrir");
        PlaySound(sonidoAbrir);
        Debug.Log($"[CHEST] Cofre {indiceCofre} abierto");
    }

    private void EjecutarAbrirConCarta()
    {
        _estado = EstadoCofre.EnTransicion;
        animator.SetTrigger("Abrir");
        PlaySound(sonidoAbrir);

        // Reactivar el Grabbable de la carta para que el jugador pueda retirarla
        if (_cartaObjActual != null)
            _cartaObjActual.GetComponent<Grabbable>().enabled = true;

        // Notificar al manager que el cofre fue reabierto (carta retirada provisionalmente)
        PlacementStateManager.Instance.RetirarCarta(indiceCofre);

        Debug.Log($"[CHEST] Cofre {indiceCofre} reabierto, carta disponible para retirar");
    }

    private void EjecutarCerrar()
    {
        _estado = EstadoCofre.EnTransicion;
        animator.SetTrigger("Cerrar");
        PlaySound(sonidoCerrar);
        Debug.Log($"[CHEST] Cofre {indiceCofre} cerrado vacío");
    }

    private void EjecutarCerrarConCarta()
    {
        _estado = EstadoCofre.EnTransicion;
        animator.SetTrigger("Cerrar");
        PlaySound(sonidoCerrar);
        DialogueManager.Instance.ReproducirCierreCofre(dialogoCofre);
        
        // Desactivar Grabbable para que no pueda agarrar la carta sin abrir el cofre
        if (_cartaObjActual != null)
            _cartaObjActual.GetComponent<Grabbable>().enabled = false;

        // Registrar en el manager
        PlacementStateManager.Instance.ColocarCarta(indiceCofre, _cartaActual.cartaID);

        Debug.Log($"[CHEST] Cofre {indiceCofre} cerrado con carta: {_cartaActual.cartaID}");
    }

    // ── Detección de carta entrando al snap point ───────────────────

    // Este método es llamado por un trigger collider en el snapPoint
    // El snap point debe tener un componente SnapPointDetector separado
    public void OnCartaEntroEnSnapPoint(GameObject cartaObj)
    {
        if (_estado != EstadoCofre.AbiertoVacio && _estado != EstadoCofre.AbiertoOcupado)
            return;

        CartaSO nuevaCarta = cartaObj.GetComponent<CartaItem>()?.cartaSO;
        if (nuevaCarta == null) return;

        // Si había una carta antes, la devolvemos al mundo
        if (_cartaObjActual != null && _cartaObjActual != cartaObj)
            ExpulsarCartaActual();

        // Asignar carta nueva
        _cartaActual = nuevaCarta;
        _cartaObjActual = cartaObj;
        _estado = EstadoCofre.AbiertoOcupado;

        // Snap: posicionar y desactivar física de la carta
        Rigidbody rb = cartaObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        cartaObj.transform.position = snapPoint.position;
        cartaObj.transform.rotation = snapPoint.rotation;

        PlaySound(sonidoCartaColocada);
        Debug.Log($"[CHEST] Carta {nuevaCarta.cartaID} snapeada en cofre {indiceCofre}");
    }

    public void OnCartaSalioDelSnapPoint(GameObject cartaObj)
    {
        if (_cartaObjActual != cartaObj) return;

        // Reactivar física de la carta
        Rigidbody rb = cartaObj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        _cartaActual = null;
        _cartaObjActual = null;
        _estado = EstadoCofre.AbiertoVacio;

        PlaySound(sonidoCartaRetirada);
        Debug.Log($"[CHEST] Carta retirada del cofre {indiceCofre}");
    }

    private void ExpulsarCartaActual()
    {
        Rigidbody rb = _cartaObjActual.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * 1.5f, ForceMode.Impulse);
        }
        _cartaObjActual.GetComponent<Grabbable>().enabled = true;
        _cartaActual = null;
        _cartaObjActual = null;
    }
    // Llamado por Animation Event al final de la animación de abrir
    public void OnAnimacionAbrirTerminada()
    {
        _estado = _cartaActual != null 
            ? EstadoCofre.AbiertoOcupado 
            : EstadoCofre.AbiertoVacio;
        Debug.Log($"[CHEST] Animación abrir completa, cofre {indiceCofre}");
    }

// Llamado por Animation Event al final de la animación de cerrar
    public void OnAnimacionCerrarTerminada()
    {
        _estado = _cartaActual != null 
            ? EstadoCofre.CerradoOcupado 
            : EstadoCofre.CerradoVacio;
        Debug.Log($"[CHEST] Animación cerrar completa, cofre {indiceCofre}");
    }

    // ── Utilidades ──────────────────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar zona de interacción en el editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}