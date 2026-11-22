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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void Configurar(
        Vector3 nuevaDireccion,
        float nuevaVelocidad,
        float nuevoDano,
        float nuevoTiempoVida)
    {
        direccion = nuevaDireccion.normalized;
        velocidad = nuevaVelocidad;
        dano = nuevoDano;
        tiempoVida = nuevoTiempoVida;

        if (direccion.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direccion);

        Destroy(gameObject, tiempoVida);
    }

    private void FixedUpdate()
    {
        if (impacto || rb == null)
            return;

        rb.linearVelocity = direccion * velocidad;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
            ProcesarImpacto(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcesarImpacto(other);
    }

    private void ProcesarImpacto(Collider objetivo)
    {
        if (impacto || objetivo == null)
            return;

        Debug.Log(
            "PROYECTIL ENEMIGO CHOCO CON: " + objetivo.name +
            " | Objeto padre: " + objetivo.transform.root.name
        );

        // Ignorar cualquier parte de un enemigo.
        if (objetivo.GetComponentInParent<EnemyController>() != null)
            return;

        // Ignorar las zonas de colocacion de torretas.
        Transform actual = objetivo.transform;
        while (actual != null)
        {
            if (actual.name.StartsWith("ZonaTorre"))
            {
                Debug.Log("PROYECTIL IGNORO ZONA DE COLOCACION: " + actual.name);
                return;
            }

            actual = actual.parent;
        }

        // Dañar al jugador.
        PlayerHealth vidaJugador = objetivo.GetComponentInParent<PlayerHealth>();
        if (vidaJugador != null)
        {
            impacto = true;
            vidaJugador.RecibirDanio(dano);
            Debug.Log("PROYECTIL ENEMIGO IMPACTO AL JUGADOR | Dano: " + dano);
            Destroy(gameObject);
            return;
        }

        // Dañar torretas.
        TurretHealth vidaTorreta = objetivo.GetComponentInParent<TurretHealth>();
        if (vidaTorreta != null)
        {
            impacto = true;
            vidaTorreta.RecibirDanio(dano);
            Debug.Log(
                "PROYECTIL ENEMIGO IMPACTO UNA TORRETA: " +
                vidaTorreta.gameObject.name + " | Dano: " + dano
            );
            Destroy(gameObject);
            return;
        }

        // Dañar el nucleo/objetivo.
        ObjectiveHealth vidaObjetivo = objetivo.GetComponentInParent<ObjectiveHealth>();
        if (vidaObjetivo != null)
        {
            impacto = true;
            vidaObjetivo.RecibirDanio(dano);
            Debug.Log("PROYECTIL ENEMIGO IMPACTO AL OBJETIVO | Dano: " + dano);
            Destroy(gameObject);
            return;
        }

        // Cualquier otro collider es un obstaculo.
        impacto = true;
        Debug.Log("PROYECTIL ENEMIGO IMPACTO UN OBSTACULO: " + objetivo.name);
        Destroy(gameObject);
    }
}
