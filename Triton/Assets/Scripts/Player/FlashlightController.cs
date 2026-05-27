using UnityEngine;

/// <summary>
/// SETUP EN UNITY:
/// 1. En la jerarquía del OVRCameraRig, busca:
///    OVRCameraRig → TrackingSpace → RightControllerAnchor (o Left según prefieras)
/// 2. Crea un GameObject hijo llamado "Linterna" con un componente Light (tipo Spot)
/// 3. Orienta la Light para que apunte hacia adelante (Z+)
/// 4. Agrega este script a cualquier GameObject activo en escena (ej. el Player)
/// 5. Arrastra la Light al campo flashlight del Inspector
///
/// BOTÓN POR DEFECTO: Botón Y (mano izquierda) o B (mano derecha) → One/Two según mano
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [Header("Referencia")]
    [Tooltip("El componente Light hijo del controller anchor")]
    public Light flashlight;

    [Header("Botón de activación")]
    [Tooltip("Botón que activa/desactiva la linterna")]
    public OVRInput.Button toggleButton = OVRInput.Button.Two;   // Y (izq) o B (der)
    [Tooltip("Controller al que pertenece el botón")]
    public OVRInput.Controller controller = OVRInput.Controller.LTouch;

    [Header("Propiedades de la linterna")]
    public float intensity    = 2f;
    public float range        = 15f;
    public float spotAngle    = 45f;
    public Color lightColor   = Color.white;

    [Header("Debug")]
    public bool isOn = false;

    void Start()
    {
        if (flashlight == null)
        {
            Debug.LogError("[LINTERNA] flashlight es NULL - arrastra la Light en el Inspector");
            return;
        }

        // Aplicar configuración inicial
        flashlight.type      = LightType.Spot;
        flashlight.intensity = intensity;
        flashlight.range     = range;
        flashlight.spotAngle = spotAngle;
        flashlight.color     = lightColor;
        flashlight.gameObject.SetActive(false);   // empieza apagada

        Debug.Log("[LINTERNA] Lista - presiona " + toggleButton + " para activar");
    }

    void Update()
    {
        if (flashlight == null) return;

        // GetDown = detecta solo el frame en que se presiona (no se mantiene)
        if (OVRInput.GetDown(toggleButton, controller))
        {
            isOn             = !isOn;
            if (isOn)
            {
                flashlight.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.Linterna, flashlight.transform.position);
            }
            else
                flashlight.gameObject.SetActive(false);  
            
            Debug.Log($"[LINTERNA] {(isOn ? "Encendida" : "Apagada")}");
        }
    }

#if UNITY_EDITOR
    // En editor sin headset: tecla F para testear
    void LateUpdate()
    {
        if (flashlight == null) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn               = !isOn;
            flashlight.enabled = isOn;
            Debug.Log($"[LINTERNA] EDITOR - {(isOn ? "Encendida" : "Apagada")}");
        }
    }
#endif
}
