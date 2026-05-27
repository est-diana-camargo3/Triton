using UnityEngine;

public class Cardumen : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject pezPrefab;
    public int cantidadPeces = 30;
    public float radioCirculo = 5f;
    public float velocidadGiro = 20f;
    public float variacionVertical = 0.8f;
    public float variacionRadio = 1.5f;
    public float variacionVelocidad = 0.3f;
    public float variacionEscala = 0.3f;

    private GameObject[] peces;
    private float[] velocidades;

    void Start()
    {
        peces = new GameObject[cantidadPeces];
        velocidades = new float[cantidadPeces];

        for (int i = 0; i < cantidadPeces; i++)
        {
            float angulo = (360f / cantidadPeces) * i + Random.Range(-10f, 10f);
            float rad = angulo * Mathf.Deg2Rad;
            float radio = radioCirculo + Random.Range(-variacionRadio, variacionRadio);

            Vector3 pos = new Vector3(
                Mathf.Cos(rad) * radio,
                Random.Range(-variacionVertical, variacionVertical),
                Mathf.Sin(rad) * radio
            );

            peces[i] = Instantiate(pezPrefab, transform.position + pos, Quaternion.identity, transform);

            float escala = 1f + Random.Range(-variacionEscala, variacionEscala);
            peces[i].transform.localScale = Vector3.one * escala;

            velocidades[i] = velocidadGiro + Random.Range(
                -velocidadGiro * variacionVelocidad,
                velocidadGiro * variacionVelocidad
            );
        }
    }

    void Update()
    {
        for (int i = 0; i < cantidadPeces; i++)
        {
            if (peces[i] == null) continue;

            peces[i].transform.RotateAround(
                transform.position,
                Vector3.up,
                velocidades[i] * Time.deltaTime
            );

            // Orientar hacia la dirección de nado (tangente al círculo)
            Vector3 alCentro = transform.position - peces[i].transform.position;
            alCentro.y = 0;
            Vector3 tangente = Vector3.Cross(Vector3.up, alCentro.normalized);

            if (tangente != Vector3.zero)
                peces[i].transform.rotation = Quaternion.LookRotation(tangente);
        }
    }
}