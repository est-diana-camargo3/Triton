using UnityEngine;

[CreateAssetMenu(fileName = "Dialogo_Nuevo", menuName = "Tritón/Diálogo")]
public class DialogueSO : ScriptableObject
{
    [Header("Contextuales — por cartas colocadas")]
    public AudioClip lineaCeroCartas;
    public AudioClip lineaUnaCarta;
    public AudioClip lineaDosCarta;
    public AudioClip lineaTresCartas;

    [Header("Random — cuando el contextual ya fue dicho")]
    public AudioClip[] lineasRandom;

    [Header("Eventos — para cofres")]
    public AudioClip lineaAlEntrarTrigger;
    public AudioClip lineaAlCerrarConCarta;

    // Devuelve la línea contextual según cuántas cartas hay colocadas
    public AudioClip ObtenerContextual(int cartasColocadas)
    {
        return cartasColocadas switch
        {
            0 => lineaCeroCartas,
            1 => lineaUnaCarta,
            2 => lineaDosCarta,
            3 => lineaTresCartas,
            _ => null
        };
    }

    // Devuelve una línea random, null si no hay ninguna configurada
    public AudioClip ObtenerRandom()
    {
        if (lineasRandom == null || lineasRandom.Length == 0) return null;
        return lineasRandom[Random.Range(0, lineasRandom.Length)];
    }
}
