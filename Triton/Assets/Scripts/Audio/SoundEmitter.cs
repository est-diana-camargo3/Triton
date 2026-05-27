using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Header("Sonido")]
    [SerializeField] private SoundData sonido;

    [Header("Tiempo")]
    [SerializeField] private bool usarTiempoAleatorio = true;
    [SerializeField] private bool NoUsaTimer = false;
    
    [SerializeField] private float tiempoFijo = 5f;

    [SerializeField] private float tiempoMin = 3f;
    [SerializeField] private float tiempoMax = 8f;

    private float timerActual;

    private void Start()
    {
        ReiniciarTimer();
    }

    private void Update()
    {
        if (NoUsaTimer)
        {
            ReproducirSonido();
        }
        else
        {
            timerActual -= Time.deltaTime;

            if (timerActual <= 0f)
            {
                ReproducirSonido();
                ReiniciarTimer();
            }
        }
    
    }

    private void ReproducirSonido()
    {
        if (sonido == null)
            return;

        AudioManager.Instance.PlaySFX(sonido,this.transform.position);
    }

    private void ReiniciarTimer()
    {
        if (usarTiempoAleatorio)
        {
            timerActual = Random.Range(tiempoMin, tiempoMax);
        }
        else
        {
            timerActual = tiempoFijo;
        }
    }
}