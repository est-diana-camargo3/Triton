using UnityEngine;

[CreateAssetMenu(fileName = "Carta_Nueva", menuName = "Tritón/Carta")]
public class CartaSO : ScriptableObject
{
    [Header("Identidad")]
    public CartaID cartaID;
    public string nombreCarta;
    public Sprite iconoCarta; // opcional por ahora, útil para UI futura
}