using UnityEngine;
using System;
using System.Collections.Generic;

public class PlacementStateManager : MonoBehaviour
{
    public static PlacementStateManager Instance { get; private set; }

    public static event Action<int> OnCartaColocada;

    // Clave: índice del cofre → carta que tiene actualmente
    private Dictionary<int, CartaID> _estadoCofres = new();

    // Clave: índice del cofre → carta que espera (registrado por cada ChestController)
    private Dictionary<int, CartaID> _idEsperadoPorCofre = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Llamado por cada ChestController al inicializarse
    // Ahora acepta cualquier índice — 0 a N cofres
    public void RegistrarCofre(int indiceCofre, CartaID idEsperado)
    {
        _idEsperadoPorCofre[indiceCofre] = idEsperado;
        _estadoCofres[indiceCofre] = CartaID.Ninguna;
        Debug.Log($"[PLACEMENT] Cofre {indiceCofre} registrado, espera: {idEsperado}");
    }

    public void ColocarCarta(int indiceCofre, CartaID idCarta)
    {
        if (!_estadoCofres.ContainsKey(indiceCofre)) return;
        _estadoCofres[indiceCofre] = idCarta;

        Debug.Log($"[PLACEMENT] Cofre {indiceCofre} recibió: {idCarta} | Cartas totales: {CartasColocadas()}/3");

        OnCartaColocada?.Invoke(CartasColocadas());

        if (TodosLosCofresLlenos())
            GameFlowManager.Instance.OnTodosCofresLlenos();
    }

    public void RetirarCarta(int indiceCofre)
    {
        if (!_estadoCofres.ContainsKey(indiceCofre)) return;
        _estadoCofres[indiceCofre] = CartaID.Ninguna;

        Debug.Log($"[PLACEMENT] Cofre {indiceCofre} vaciado | Cartas totales: {CartasColocadas()}/3");

        OnCartaColocada?.Invoke(CartasColocadas());
    }

    // Las 3 cartas fueron colocadas en ALGÚN cofre — no importa cuál
    public bool TodosLosCofresLlenos()
    {
        return CartasColocadas() >= 3;
    }

    // Cuántas cartas hay colocadas en total (sin importar si están bien o mal)
    public int CartasColocadas()
    {
        int contador = 0;
        foreach (var kvp in _estadoCofres)
            if (kvp.Value != CartaID.Ninguna) contador++;
        return contador;
    }

    // Cuántas cartas están en el cofre correcto
    public EndingType ResolverFinal()
    {
        int correctas = 0;

        foreach (var kvp in _estadoCofres)
        {
            int indice = kvp.Key;
            CartaID cartaColocada = kvp.Value;

            if (cartaColocada != CartaID.Ninguna &&
                _idEsperadoPorCofre.TryGetValue(indice, out CartaID esperada) &&
                cartaColocada == esperada)
            {
                correctas++;
            }
        }

        Debug.Log($"[ENDING] Cartas correctas: {correctas}/3");

        return correctas switch
        {
            3 => EndingType.Final_A,
            2 => EndingType.Final_B,
            _ => EndingType.Final_C
        };
    }
}