using UnityEngine;

public class Shotgun : HitscanWeapon
{
    [Header("Escopeta")]
    [SerializeField] private int perdigones = 8;
    [SerializeField] private float dispersion = 6f;

    public override void Disparar()
    {
        if (!PuedeDisparar())
            return;

        // Disparar todos los perdigones
        for (int i = 0; i < perdigones; i++)
        {
            DispararRayo(
                ObtenerDireccionConDispersion()
            );
        }

        // Retroceso del jugador
        // una sola vez por disparo
        AplicarRetrocesoJugador();
    }

    protected override void ProcesarImpacto(
        RaycastHit hit,
        Vector3 direccionDisparo)
    {
        // Solo buscamos enemigos
        EnemyHealth enemigo =
            hit.collider.GetComponentInParent<EnemyHealth>();

        if (enemigo == null)
            return;

        // Repartimos el daño total
        // entre los perdigones
        float danoPorPerdigon =
            dańo / perdigones;

        enemigo.RecibirDanio(
            danoPorPerdigon
        );

        // Buscar controlador del enemigo
        EnemyController enemyController =
            enemigo.GetComponent<EnemyController>();

        if (enemyController != null)
        {
            // También repartimos el empuje
            float empujePorPerdigon =
                fuerzaEmpujeEnemigo /
                perdigones;

            enemyController.RecibirEmpuje(
                direccionDisparo,
                empujePorPerdigon
            );
        }
    }

    private Vector3 ObtenerDireccionConDispersion()
    {
        Vector3 direccion =
            camara.transform.forward;

        direccion +=
            camara.transform.right *
            Random.Range(
                -dispersion,
                dispersion
            ) / 100f;

        direccion +=
            camara.transform.up *
            Random.Range(
                -dispersion,
                dispersion
            ) / 100f;

        return direccion.normalized;
    }
}