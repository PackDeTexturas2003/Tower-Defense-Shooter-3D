using UnityEngine;

public class BalaTorreta : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float velocidad = 20f;
    [SerializeField] private float distanciaImpacto = 0.5f;
    [SerializeField] private float tiempoMaximo = 5f;

    private Transform objetivo;
    private float daño;
    private bool yaImpacto;

    public void Configurar(Transform nuevoObjetivo, float nuevoDaño)
    {
        objetivo = nuevoObjetivo;
        daño = nuevoDaño;

        Destroy(gameObject, tiempoMaximo);
    }

    private void Update()
    {
        if (yaImpacto)
            return;

        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direccion =
            (objetivo.position - transform.position).normalized;

        transform.position +=
            direccion * velocidad * Time.deltaTime;

        if (direccion != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direccion);
        }

        float distancia =
            Vector3.Distance(
                transform.position,
                objetivo.position
            );

        if (distancia <= distanciaImpacto)
        {
            Impactar();
        }
    }

    private void Impactar()
    {
        if (yaImpacto)
            return;

        yaImpacto = true;

        EnemyHealth enemigo =
            objetivo.GetComponentInParent<EnemyHealth>();

        if (enemigo != null)
        {
            enemigo.RecibirDanio(daño);
        }

        Destroy(gameObject);
    }
}