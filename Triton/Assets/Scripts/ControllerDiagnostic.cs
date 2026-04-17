using UnityEngine;

public class ControllerDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[DIAG] Escena cargada correctamente");
        Debug.Log($"[DIAG] OVRPlugin version: {OVRPlugin.version}");
    }

    void Update()
    {
        // Verifica qué controllers están conectados
        bool leftConnected  = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
        bool rightConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
        bool handsConnected = OVRInput.IsControllerConnected(OVRInput.Controller.Hands);

        // Solo loguea cada 2 segundos para no saturar logcat
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[DIAG] Left controller:  {leftConnected}");
            Debug.Log($"[DIAG] Right controller: {rightConnected}");
            Debug.Log($"[DIAG] Hand tracking:    {handsConnected}");
            Debug.Log($"[DIAG] Active controller: {OVRInput.GetActiveController()}");

            Vector3 leftVel  = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
            Vector3 rightVel = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            Debug.Log($"[DIAG] Left vel: {leftVel}  Right vel: {rightVel}");
        }
    }
}