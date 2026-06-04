using UnityEngine;
using System;

public class LocomotionManager : MonoBehaviour
{
    public static LocomotionManager Instance { get; private set; }

    public static event Action<LocomotionMode> OnModoChanged;

    private const string PREFS_KEY = "LocomotionMode";

    public LocomotionMode ModoActual { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar preferencia guardada — default: Brazadas
        ModoActual = (LocomotionMode)PlayerPrefs.GetInt(PREFS_KEY, 0);
        Debug.Log($"[LOCOMOTION] Modo cargado: {ModoActual}");
    }

    public void SetModo(LocomotionMode modo)
    {
        if (ModoActual == modo) return;

        ModoActual = modo;
        PlayerPrefs.SetInt(PREFS_KEY, (int)modo);
        PlayerPrefs.Save();

        OnModoChanged?.Invoke(modo);
        Debug.Log($"[LOCOMOTION] Modo cambiado: {modo}");
    }
}