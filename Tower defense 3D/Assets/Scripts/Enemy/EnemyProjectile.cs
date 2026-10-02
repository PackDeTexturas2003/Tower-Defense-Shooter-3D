using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    private Vector3 direccion;

    private float velocidad;

    private float dano;

    private float tiempoVida;

    private Rigidbody rb;

    private bool impacto;

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();

        rb.useGravity = false;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;
    }

    // =========================================
    // CONFIGURAR
    // =========================================

    public void Configurar(
        Vector3 nuevaDireccion,
        float nuevaVelocidad,
        float nuevoDano,
        float nuevoTiempoVida)
    {
        direccion =
            nuevaDireccion.normalized;

        velocidad =
            nuevaVelocidad;

        dano =
            nuevoDano;

        tiempoVida =
            nuevoTiempoVida;

        transform.rotation =
            Quaternion.LookRotation(
                direccion
            );

        Destroy(
            gameObject,
            tiempoVida
        );
    }

    // =========================================
    // UPDATE
    // =========================================

    private void FixedUpdate()
    {
        if (impacto)
            return;

        rb.linearVelocity =
            direccion *
            velocidad;
    }

    // =========================================
    // COLISION
    // =========================================

    private void OnCollisionEnter(
        Collision collision)
    {
        ProcesarImpacto(
            collision.collider
        );
    }

    // =========================================
    // TRIGGER
    // =========================================

    private void OnTriggerEnter(
        Collider other)
    {
        ProcesarImpacto(
            other
        );
    }

    // =========================================
    // PROCESAR IMPACTO
    // =========================================

    private void ProcesarImpacto(
        Collider objetivo)
    {
        if (impacto)
            return;

        if (objetivo == null)
            return;

        // -------------------------------------
        // EVITAR QUE GOLPEE AL PROPIO ENEMIGO
        // -------------------------------------

        EnemyController enemigo =
            objetivo.GetComponentInParent<EnemyController>();

        if (enemigo != null)
        {
            return;
        }

        // -------------------------------------
        // JUGADOR
        // -------------------------------------

        PlayerHealth vidaJugador =
            objetivo.GetComponentInParent<PlayerHealth>();

        if (vidaJugador != null)
        {
            impacto = true;

            vidaJugador.RecibirDanio(
                dano
            );

            Debug.Log(
                "Proyectil enemigo impactó " +
                "al jugador | Daño: " +
                dano
            );

            Destroy(
                gameObject
            );

            return;
        }

        // -------------------------------------
        // OBJETIVO / NUCLEO
        // -------------------------------------

        ObjectiveHealth vidaObjetivo =
            objetivo.GetComponentInParent<ObjectiveHealth>();

        if (vidaObjetivo != null)
        {
            impacto = true;

            vidaObjetivo.RecibirDanio(
                dano
            );

            Debug.Log(
                "Proyectil enemigo impactó " +
                "al objetivo | Daño: " +
                dano
            );

            Destroy(
                gameObject
            );

            return;
        }

        // -------------------------------------
        // PARED / SUELO / OBSTACULO
        // -------------------------------------

        impacto = true;

        Destroy(
            gameObject
        );
    }
}