using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RPGProjectile : MonoBehaviour
{
    [Header("Visual de explosión")]
    [SerializeField] private GameObject visualExplosion;

    [SerializeField] private float duracionVisualExplosion = 0.5f;

    [Header("Audio")]
    [SerializeField] private SFXManager sfxManager;

    [Header("Tiempo de vida")]
    [SerializeField] private float tiempoMaximo = 10f;

    private float danoExplosion;
    private float radioExplosion;
    private float fuerzaExplosion;

    private bool yaExploto = false;

    // =========================================
    // CONFIGURAR DESDE EL RPG
    // =========================================

    public void Configurar(
        float dano,
        float radio,
        float fuerza)
    {
        danoExplosion = dano;
        radioExplosion = radio;
        fuerzaExplosion = fuerza;
    }

    // =========================================
    // INICIO
    // =========================================

    private void Start()
    {
        if (sfxManager == null)
        {
            sfxManager =
                FindFirstObjectByType<SFXManager>();
        }

        Destroy(
            gameObject,
            tiempoMaximo
        );
    }

    // =========================================
    // COLISIÓN
    // =========================================

    private void OnCollisionEnter(
        Collision collision)
    {
        Explota();
    }

    private void OnTriggerEnter(
        Collider other)
    {
        Explota();
    }

    // =========================================
    // EXPLOSIÓN
    // =========================================

    private void Explota()
    {
        if (yaExploto)
            return;

        yaExploto = true;

        // =====================================
        // SONIDO
        // =====================================

        if (sfxManager != null)
        {
            sfxManager.ReproducirExplosionRPG();
        }

        // =====================================
        // VISUAL
        // =====================================

        MostrarVisualExplosion();

        // =====================================
        // DETECTAR OBJETOS
        // =====================================

        Collider[] objetos =
            Physics.OverlapSphere(
                transform.position,
                radioExplosion
            );

        bool jugadorDanado = false;

        foreach (Collider objeto in objetos)
        {
            // =================================
            // JUGADOR
            // =================================

            PlayerHealth jugador =
                objeto.GetComponentInParent<PlayerHealth>();

            if (jugador == null)
            {
                jugador =
                    objeto.GetComponent<PlayerHealth>();
            }

            if (jugador != null)
            {
                if (!jugadorDanado)
                {
                    float danoJugador =
                        danoExplosion * 0.5f;

                    jugador.RecibirDanio(
                        danoJugador
                    );

                    jugadorDanado = true;

                    Debug.Log(
                        "RPG DAÑÓ AL JUGADOR | " +
                        "Daño: " +
                        danoJugador
                    );
                }

                continue;
            }

            // =================================
            // ENEMIGO
            // =================================

            EnemyHealth enemigo =
                objeto.GetComponentInParent<EnemyHealth>();

            if (enemigo == null)
            {
                enemigo =
                    objeto.GetComponent<EnemyHealth>();
            }

            if (enemigo == null)
                continue;

            enemigo.RecibirDanio(
                danoExplosion
            );

            // =================================
            // EMPUJE
            // =================================

            EnemyController enemyController =
                enemigo.GetComponent<EnemyController>();

            if (enemyController != null)
            {
                Vector3 direccion =
                    enemigo.transform.position -
                    transform.position;

                direccion.y = 0f;

                if (direccion.sqrMagnitude > 0.01f)
                {
                    direccion.Normalize();

                    enemyController.RecibirEmpuje(
                        direccion,
                        fuerzaExplosion
                    );
                }
            }
        }

        Debug.Log(
            "RPG EXPLOTÓ | Daño enemigos: " +
            danoExplosion +
            " | Daño jugador: " +
            (danoExplosion * 0.5f) +
            " | Radio: " +
            radioExplosion
        );

        Destroy(gameObject);
    }

    // =========================================
    // VISUAL DE EXPLOSIÓN
    // =========================================

    private void MostrarVisualExplosion()
    {
        if (visualExplosion == null)
            return;

        GameObject explosion =
            Instantiate(
                visualExplosion,
                transform.position,
                Quaternion.identity
            );

        float diametro =
            radioExplosion * 2f;

        explosion.transform.localScale =
            Vector3.one * diametro;

        Destroy(
            explosion,
            duracionVisualExplosion
        );
    }

    // =========================================
    // GIZMO DEL RADIO
    // =========================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radioExplosion
        );
    }
}