using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.IO;

public class CinematicController : MonoBehaviour
{
    [Header("VideoPlayer")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Header("Nombres de archivos en StreamingAssets")]
    public string archivoIntro    = "intro.mp4";
    public string archivoEndingA  = "ending_A.mp4";
    public string archivoEndingB  = "ending_B.mp4";
    public string archivoEndingC  = "ending_C.mp4";

    [Header("Fade")]
    public CanvasGroup fadeCanvas; // Canvas negro para fade in/out
    public float duracionFade = 1f;

    // ── Suscripción al patrón Observer ──────────────────────────────

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
        // Configurar VideoPlayer para usar AudioSource externo
        // Necesario para audio espacial correcto en VR
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // Preparar evento de fin de video
        videoPlayer.loopPointReached += OnVideoTerminado;

        // Empezar con fade negro
        SetFade(1f);
    }

    // ── Observer ────────────────────────────────────────────────────

    private void ManejarCambioEstado(GameState nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case GameState.CinematicIntro:
                StartCoroutine(ReproducirVideo(archivoIntro));
                break;

            case GameState.CinematicEnding:
                string archivo = ResolverArchivoEnding();
                StartCoroutine(ReproducirVideo(archivo));
                break;
        }
    }

    // ── Resolución del ending ───────────────────────────────────────

    private string ResolverArchivoEnding()
    {
        return GameFlowManager.Instance.EndingResuelto switch
        {
            EndingType.Final_A => archivoEndingA,
            EndingType.Final_B => archivoEndingB,
            EndingType.Final_C => archivoEndingC,
            _                  => archivoEndingC // fallback seguro
        };
    }

    // ── Reproducción ────────────────────────────────────────────────

    private IEnumerator ReproducirVideo(string nombreArchivo)
    {
        // Construir ruta de StreamingAssets
        string ruta = Path.Combine(Application.streamingAssetsPath, nombreArchivo);
        videoPlayer.url = ruta;

        Debug.Log($"[CINEMATIC] Preparando video: {ruta}");

        // Preparar el video sin reproducirlo aún
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        Debug.Log($"[CINEMATIC] Video preparado: {nombreArchivo}");

        // Fade in — revelar la pantalla
        yield return StartCoroutine(Fade(1f, 0f));

        videoPlayer.Play();
        Debug.Log($"[CINEMATIC] Reproduciendo: {nombreArchivo}");
    }

    // ── Evento de fin de video ──────────────────────────────────────

    private void OnVideoTerminado(VideoPlayer vp)
    {
        Debug.Log($"[CINEMATIC] Video terminado");
        StartCoroutine(FinalizarCinematica());
    }

    private IEnumerator FinalizarCinematica()
    {
        // Fade out antes de notificar al GameFlowManager
        yield return StartCoroutine(Fade(0f, 1f));

        videoPlayer.Stop();

        // Notificar según el estado actual
        switch (GameFlowManager.Instance.EstadoActual)
        {
            case GameState.CinematicIntro:
                GameFlowManager.Instance.OnIntroTerminada();
                break;

            case GameState.CinematicEnding:
                GameFlowManager.Instance.OnEndingTerminado();
                break;
        }
    }

    // ── Fade ────────────────────────────────────────────────────────

    private IEnumerator Fade(float desde, float hasta)
    {
        float tiempo = 0f;
        SetFade(desde);

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            SetFade(Mathf.Lerp(desde, hasta, tiempo / duracionFade));
            yield return null;
        }

        SetFade(hasta);
    }

    private void SetFade(float alpha)
    {
        if (fadeCanvas != null)
            fadeCanvas.alpha = alpha;
    }
}
