using UnityEngine;
using System.IO;

public class LogFile : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("OVRCameraRig → TrackingSpace → CenterEyeAnchor")]
    public Transform centerEye;

    [Tooltip("OVRCameraRig → TrackingSpace → LeftHandAnchor")]
    public Transform leftHandAnchor;

    [Tooltip("OVRCameraRig → TrackingSpace → RightHandAnchor")]
    public Transform rightHandAnchor;

    [Tooltip("El GameObject raíz del jugador (Player)")]
    public Transform playerBody;

    [Header("Frecuencia de registro")]
    [Tooltip("Segundos entre cada línea de log. 0.1 = 10 veces por segundo")]
    public float intervaloSegundos = 0.1f;

    // ── Privados ────────────────────────────────────────────────────
    public string DataPath;
    private StreamWriter _logWriter;
    private float _timerLog = 0f;

    // ── Header de columnas ──────────────────────────────────────────
    private const string HEADER =
        "Timestamp\t" +
        "LocoMode\t" +
        "Body_PosX\tBody_PosY\tBody_PosZ\t" +
        "Head_PosX\tHead_PosY\tHead_PosZ\t" +
        "Head_RotX\tHead_RotY\tHead_RotZ\t" +
        "LHand_PosX\tLHand_PosY\tLHand_PosZ\t" +
        "LHand_RotX\tLHand_RotY\tLHand_RotZ\t" +
        "RHand_PosX\tRHand_PosY\tRHand_PosZ\t" +
        "RHand_RotX\tRHand_RotY\tRHand_RotZ";

    // ── Inicialización ──────────────────────────────────────────────

    void Start()
    {
        DataPath = Application.persistentDataPath;

        string logPath = DataPath + FileSep() +
                         "Log_" + System.DateTime.Now.Ticks + ".tsv";

        _logWriter = new StreamWriter(logPath, true);
        _logWriter.WriteLine(HEADER);
        _logWriter.Flush();

        Debug.Log($"[LOG] Archivo de log creado en: {logPath}");

        LogLine("SESSION_START");
    }

    // ── Loop de registro ────────────────────────────────────────────

    void Update()
    {
        _timerLog += Time.deltaTime;

        if (_timerLog >= intervaloSegundos)
        {
            _timerLog = 0f;
            LogFrame();
        }
    }

    // ── Registro de un frame ────────────────────────────────────────

    private void LogFrame()
    {
        // Modo de locomoción actual
        string modo = LocomotionManager.Instance != null
            ? LocomotionManager.Instance.ModoActual.ToString()
            : "Unknown";

        // Posición del cuerpo
        Vector3 bodyPos = playerBody != null
            ? playerBody.position
            : Vector3.zero;

        // Cabeza
        Vector3 headPos = centerEye != null ? centerEye.position : Vector3.zero;
        Vector3 headRot = centerEye != null ? centerEye.eulerAngles : Vector3.zero;

        // Mano izquierda
        Vector3 lPos = leftHandAnchor  != null ? leftHandAnchor.position  : Vector3.zero;
        Vector3 lRot = leftHandAnchor  != null ? leftHandAnchor.eulerAngles : Vector3.zero;

        // Mano derecha
        Vector3 rPos = rightHandAnchor != null ? rightHandAnchor.position  : Vector3.zero;
        Vector3 rRot = rightHandAnchor != null ? rightHandAnchor.eulerAngles : Vector3.zero;

        string line =
            $"{Timestamp()}\t" +
            $"{modo}\t" +
            $"{F(bodyPos.x)}\t{F(bodyPos.y)}\t{F(bodyPos.z)}\t" +
            $"{F(headPos.x)}\t{F(headPos.y)}\t{F(headPos.z)}\t" +
            $"{F(headRot.x)}\t{F(headRot.y)}\t{F(headRot.z)}\t" +
            $"{F(lPos.x)}\t{F(lPos.y)}\t{F(lPos.z)}\t" +
            $"{F(lRot.x)}\t{F(lRot.y)}\t{F(lRot.z)}\t" +
            $"{F(rPos.x)}\t{F(rPos.y)}\t{F(rPos.z)}\t" +
            $"{F(rRot.x)}\t{F(rRot.y)}\t{F(rRot.z)}";

        _logWriter.WriteLine(line);
        _logWriter.Flush();
    }

    // ── Línea de evento puntual (inicio, fin, etc.) ─────────────────

    public void LogLine(string evento)
    {
        string line = $"{Timestamp()}\t{evento}";
        _logWriter.WriteLine(line);
        _logWriter.Flush();
    }

    // ── Cierre limpio del archivo ───────────────────────────────────

    private void OnApplicationQuit()
    {
        LogLine("SESSION_END");

        if (_logWriter != null)
        {
            _logWriter.Flush();
            _logWriter.Close();
            Debug.Log("[LOG] Archivo de log cerrado correctamente");
        }
    }

    // ── Utilidades ──────────────────────────────────────────────────

    private string Timestamp()
    {
        var n = System.DateTime.Now;
        return $"{n.Date:yyyy-MM-dd}\t{n.Hour}\t{n.Minute}\t{n.Second}\t{n.Millisecond}";
    }

    private string F(float v) => v.ToString("F4");

    public static char FileSep()
    {
        return (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsServer)
            ? '/'
            : '/';
    }
}