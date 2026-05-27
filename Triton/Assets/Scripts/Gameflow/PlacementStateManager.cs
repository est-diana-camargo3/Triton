using UnityEngine;
using System;
public class PlacementStateManager : MonoBehaviour
{
    public static PlacementStateManager Instance { get; private set; }

    public static event Action<int> OnCartaColocada;
    
    // Qué carta está en cada cofre (índice 0-2, Ninguna si está vacío)
    private CartaID[] _estadoCofres = new CartaID[3];

    // Qué ID de carta corresponde a cada cofre (se asigna en el Inspector vía ChestController)
    // Índice 0 = cofre que espera Tridente, índice 1 = cofre que espera Ballena, etc.
    // Esto lo registran los propios ChestController al inicializarse
    private CartaID[] _idEsperadoPorCofre = new CartaID[3];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Estado inicial: ningún cofre ocupado
        for (int i = 0; i < 3; i++)
            _estadoCofres[i] = CartaID.Ninguna;
    }

    // Llamado por ChestController al inicializarse para registrar qué carta espera
    public void RegistrarCofre(int indiceCofre, CartaID idEsperado)
    {
        if (indiceCofre < 0 || indiceCofre >= 3) return;
        _idEsperadoPorCofre[indiceCofre] = idEsperado;
    }

    // Llamado por ChestController cuando el jugador coloca una carta
    public void ColocarCarta(int indiceCofre, CartaID idCarta)
    {
        if (indiceCofre < 0 || indiceCofre >= 3) return;
        _estadoCofres[indiceCofre] = idCarta;
        
        if (TodosLosCofresLlenos())
            GameFlowManager.Instance.OnTodosCofresLlenos();

        Debug.Log($"[PLACEMENT] Cofre {indiceCofre} recibió carta: {idCarta}");
        OnCartaColocada?.Invoke(CartasColocadas());
    }

    // Llamado por ChestController cuando el jugador retira una carta
    public void RetirarCarta(int indiceCofre)
    {
        if (indiceCofre < 0 || indiceCofre >= 3) return;
        _estadoCofres[indiceCofre] = CartaID.Ninguna;

        Debug.Log($"[PLACEMENT] Cofre {indiceCofre} vaciado");
        OnCartaColocada?.Invoke(CartasColocadas());
    }

    // Consulta si los 3 cofres de carta tienen algo (no necesariamente correcto)
    public bool TodosLosCofresLlenos()
    {
        for (int i = 0; i < 3; i++)
            if (_estadoCofres[i] == CartaID.Ninguna) return false;
        return true;
    }

    // Cuenta cartas correctamente colocadas y devuelve el final correspondiente
    public EndingType ResolverFinal()
    {
        int correctas = 0;

        for (int i = 0; i < 3; i++)
        {
            if (_estadoCofres[i] != CartaID.Ninguna &&
                _estadoCofres[i] == _idEsperadoPorCofre[i])
                correctas++;
        }

        Debug.Log($"[ENDING] Cartas correctas: {correctas}/3");

        return correctas switch
        {
            3 => EndingType.Final_A,
            2 => EndingType.Final_B,
            _ => EndingType.Final_C
        };
    }
    // Devuelve cuántos cofres tienen una carta (sin importar si es correcta)
    public int CartasColocadas()
    {
        int contador = 0;
        for (int i = 0; i < 3; i++)
            if (_estadoCofres[i] != CartaID.Ninguna) contador++;
        return contador;
    }
}
