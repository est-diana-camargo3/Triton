using UnityEngine;
using System.IO;

public class LogFile : MonoBehaviour
{
    [Header("Referencias")]
    public Transform centerEye;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public Transform playerBody;

    [Header("Frecuencia de registro")]
    public float intervaloSegundos = 0.1f;

    // ── Privados ─────────────────────────────────────────────────────
    public string DataPath;
    private StreamWriter _logWriter;
    private float _timerLog = 0f;

    // ── Columnas del header ──────────────────────────────────────────
    private const string HEADER =
        "Date\t" +
        "Hour\tMin\tSec\tMs\t" +
        "LocoMode\t" +
        "DataType\t" +
        "X\tY\tZ";

    // ── Inicialización ───────────────────────────────────────────────

    private void Start()
    {
        DataPath = Application.persistentDataPath;

        string logPath = DataPath + FileSep() +
                         "Log_" + System.DateTime.Now.Ticks + ".tsv";

        _logWriter = new StreamWriter(logPath, true);
        _logWriter.WriteLine(HEADER);
        _logWriter.Flush();

        Debug.Log($"[LOG] Archivo creado en: {logPath}");
        LogEvento("SESSION_START");
    }

    // ── Loop de registro ─────────────────────────────────────────────

    private void Update()
    {
        _timerLog += Time.deltaTime;

        if (_timerLog >= intervaloSegundos)
        {
            _timerLog = 0f;
            LogFrame();
        }
    }

    // ── Registro de un frame — una línea por vector ──────────────────

    private void LogFrame()
    {
        string ts   = Timestamp();
        string modo = LocomotionManager.Instance != null
                      ? LocomotionManager.Instance.ModoActual.ToString()
                      : "Unknown";

        // Cuerpo
        LogVector(ts, modo, "Body_Pos",
            playerBody != null ? playerBody.position : Vector3.zero);

        // Cabeza
        LogVector(ts, modo, "Head_Pos",
            centerEye != null ? centerEye.position : Vector3.zero);

        LogVector(ts, modo, "Head_Rot",
            centerEye != null ? centerEye.eulerAngles : Vector3.zero);

        // Mano izquierda
        LogVector(ts, modo, "LHand_Pos",
            leftHandAnchor != null ? leftHandAnchor.position : Vector3.zero);

        LogVector(ts, modo, "LHand_Rot",
            leftHandAnchor != null ? leftHandAnchor.eulerAngles : Vector3.zero);

        // Mano derecha
        LogVector(ts, modo, "RHand_Pos",
            rightHandAnchor != null ? rightHandAnchor.position : Vector3.zero);

        LogVector(ts, modo, "RHand_Rot",
            rightHandAnchor != null ? rightHandAnchor.eulerAngles : Vector3.zero);
    }

    // ── Escritura de una línea por vector ────────────────────────────

    private void LogVector(string timestamp, string modo, string tipo, Vector3 v)
    {
        string line = $"{timestamp}\t{modo}\t{tipo}\t{F(v.x)}\t{F(v.y)}\t{F(v.z)}";
        _logWriter.WriteLine(line);
    }

    // ── Eventos puntuales (inicio, fin, cambio de modo) ──────────────

    public void LogEvento(string evento)
    {
        // Los eventos usan el mismo formato pero sin XYZ
        // Se rellenan con guiones para mantener columnas alineadas
        string modo = LocomotionManager.Instance != null
                      ? LocomotionManager.Instance.ModoActual.ToString()
                      : "-";

        string line = $"{Timestamp()}\t{modo}\t{evento}\t-\t-\t-";
        _logWriter.WriteLine(line);
        _logWriter.Flush();
    }

    // ── Cierre limpio ────────────────────────────────────────────────

    private void OnApplicationQuit()
    {
        LogEvento("SESSION_END");

        if (_logWriter != null)
        {
            _logWriter.Flush();
            _logWriter.Close();
            Debug.Log("[LOG] Archivo cerrado correctamente");
        }
    }

    // ── Utilidades ───────────────────────────────────────────────────

    private string Timestamp()
    {
        var n = System.DateTime.Now;
        return $"{n:yyyy-MM-dd}\t{n.Hour}\t{n.Minute}\t{n.Second}\t{n.Millisecond}";
    }

    private string F(float v) => v.ToString("F4");

    public static char FileSep() => '/';
}