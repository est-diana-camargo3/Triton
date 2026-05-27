using UnityEngine;

public class SnapPointDetector : MonoBehaviour
{
    public ChestController cofre; // referencia al padre

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Carta"))
            cofre.OnCartaEntroEnSnapPoint(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Carta"))
            cofre.OnCartaSalioDelSnapPoint(other.gameObject);
    }
}
