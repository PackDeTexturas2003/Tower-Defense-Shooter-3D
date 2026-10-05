using UnityEngine;

public class TorretaCortoAlcance : MonoBehaviour
{
    [Header("Configuración de ataque")]
    [SerializeField] private float rango = 6f;
    [SerializeField] private float daño = 40f;
    [SerializeField] private float tiempoEntreDisparos = 0.8f;

    [Header("Bala")]
    [SerializeField] private GameObject balaTorreta;
    [SerializeField] private Transform puntoDisparo;

    [Header("Detección de enemigos")]
    [SerializeField] private string tagEnemigo = "Enemy";

    private Transform objetivoActual;
    private float tiempoProximoDisparo;

    private void Update()
    {
        BuscarObjetivo();

        if (objetivoActual == null)
            return;

        if (Vector3.Distance(transform.position, objetivoActual.position) > rango)
        {
            objetivoActual = null;
            return;
        }

        if (Time.time >= tiempoProximoDisparo)
        {
            Disparar();
            tiempoProximoDisparo = Time.time + tiempoEntreDisparos;
        }
    }

    private void BuscarObjetivo()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);

        float distanciaMasCercana = Mathf.Infinity;
        Transform mejorObjetivo = null;

        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector3.Distance(
                transform.position,
                enemigo.transform.position
            );

            if (distancia <= rango && distancia < distanciaMasCercana)
            {
                distanciaMasCercana = distancia;
                mejorObjetivo = enemigo.transform;
            }
        }

        objetivoActual = mejorObjetivo;
    }

    private void Disparar()
    {
        if (balaTorreta == null || puntoDisparo == null)
            return;

        GameObject nuevaBala = Instantiate(
            balaTorreta,
            puntoDisparo.position,
            Quaternion.identity
        );

        BalaTorreta bala = nuevaBala.GetComponent<BalaTorreta>();

        if (bala != null)
        {
            bala.Configurar(objetivoActual, daño);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            rango
        );
    }
}