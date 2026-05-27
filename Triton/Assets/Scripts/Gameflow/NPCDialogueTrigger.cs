using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    public DialogueSO dialogo;

    // Cooldown para no disparar diálogo cada vez que el jugador
    // entra y sale repetidamente de la zona
    [Header("Cooldown entre activaciones")]
    public float cooldownSegundos = 8f;
    private float _tiempoUltimaActivacion = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - _tiempoUltimaActivacion < cooldownSegundos) return;

        _tiempoUltimaActivacion = Time.time;
        DialogueManager.Instance.ReproducirPorProximidad(dialogo);
    }
}
