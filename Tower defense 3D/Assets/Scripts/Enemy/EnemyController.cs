using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum TipoObjetivo
    {
        Jugador,
        Objetivo
    }

    [Header("Datos")]
    [SerializeField] private EnemyData data;

    [Header("Tipo de objetivo")]
    [SerializeField]
    private TipoObjetivo tipoObjetivo =
        TipoObjetivo.Jugador;

    [Header("Objetivo")]
    [SerializeField] private Transform objetivo;

    private CharacterController controller;
    private NavMeshAgent agent;

    private float siguienteAtaque;

    private bool agentePreparado;

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        agent =
            GetComponent<NavMeshAgent>();

        // El NavMeshAgent controla el movimiento.
        // El CharacterController se mantiene
        // como componente del enemigo.
        controller.enabled = false;
    }

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        ConfigurarAgente();

        BuscarObjetivo();
    }

    // =========================================
    // CONFIGURAR NAVMESH
    // =========================================

    private void ConfigurarAgente()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                gameObject.name +
                " no está colocado sobre el NavMesh."
            );

            return;
        }

        agentePreparado = true;

        // IMPORTANTE:
        // La velocidad ahora se configura
        // directamente desde NavMeshAgent.
        //
        // Ya NO utilizamos:
        // data.velocidad

        agent.angularSpeed = 360f;

        agent.acceleration = 20f;

        agent.stoppingDistance =
            data != null
            ? data.distanciaAtaque
            : 1.5f;

        agent.autoBraking = true;

        agent.autoRepath = true;

        agent.updateRotation = true;

        agent.updatePosition = true;
    }

    // =========================================
    // UPDATE
    // =========================================

    private void Update()
    {
        if (data == null)
            return;

        if (!agentePreparado)
        {
            if (agent != null &&
                agent.isOnNavMesh)
            {
                ConfigurarAgente();
            }

            return;
        }

        if (objetivo == null)
        {
            BuscarObjetivo();

            return;
        }

        Comportamiento();
    }

    // =========================================
    // BUSCAR OBJETIVO
    // =========================================

    private void BuscarObjetivo()
    {
        GameObject objetoEncontrado = null;

        if (tipoObjetivo ==
            TipoObjetivo.Jugador)
        {
            objetoEncontrado =
                GameObject.FindGameObjectWithTag(
                    "Player"
                );
        }
        else if (tipoObjetivo ==
                 TipoObjetivo.Objetivo)
        {
            objetoEncontrado =
                GameObject.FindGameObjectWithTag(
                    "Objetivo"
                );
        }

        if (objetoEncontrado != null)
        {
            objetivo =
                objetoEncontrado.transform;

            Debug.Log(
                gameObject.name +
                " encontró su objetivo: " +
                objetivo.name
            );
        }
        else
        {
            string tagBuscado =
                tipoObjetivo ==
                TipoObjetivo.Jugador
                ? "Player"
                : "Objetivo";

            Debug.LogWarning(
                gameObject.name +
                " no encontró un objeto con Tag " +
                tagBuscado
            );
        }
    }

    // =========================================
    // COMPORTAMIENTO
    // =========================================

    private void Comportamiento()
    {
        if (JugadorEnRangoDeAtaque())
        {
            DetenerMovimiento();

            MirarObjetivo();

            Atacar();

            return;
        }

        PerseguirObjetivo();
    }

    // =========================================
    // COMPROBAR DISTANCIA
    // =========================================

    private bool JugadorEnRangoDeAtaque()
    {
        if (objetivo == null)
            return false;

        Vector3 posicionEnemigo =
            transform.position;

        Vector3 posicionObjetivo =
            objetivo.position;

        posicionEnemigo.y = 0f;
        posicionObjetivo.y = 0f;

        float distancia =
            Vector3.Distance(
                posicionEnemigo,
                posicionObjetivo
            );

        return distancia <=
               data.distanciaAtaque;
    }

    // =========================================
    // PERSEGUIR
    // =========================================

    private void PerseguirObjetivo()
    {
        if (objetivo == null)
            return;

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        agent.stoppingDistance =
            data.distanciaAtaque;

        agent.SetDestination(
            objetivo.position
        );
    }

    // =========================================
    // DETENER
    // =========================================

    private void DetenerMovimiento()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = true;

        agent.ResetPath();
    }

    // =========================================
    // MIRAR OBJETIVO
    // =========================================

    private void MirarObjetivo()
    {
        if (objetivo == null)
            return;

        Vector3 direccion =
            objetivo.position -
            transform.position;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        direccion.Normalize();

        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(
                direccion
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                10f *
                Time.deltaTime
            );
    }

    // =========================================
    // ATAQUE
    // =========================================

    private void Atacar()
    {
        if (Time.time < siguienteAtaque)
            return;

        if (tipoObjetivo ==
            TipoObjetivo.Jugador)
        {
            AtacarJugador();
        }
        else if (tipoObjetivo ==
                 TipoObjetivo.Objetivo)
        {
            AtacarObjetivo();
        }
    }

    // =========================================
    // ATAQUE AL JUGADOR
    // =========================================

    private void AtacarJugador()
    {
        if (objetivo == null)
            return;

        PlayerHealth vidaJugador =
            objetivo.GetComponentInParent<PlayerHealth>();

        if (vidaJugador == null)
        {
            vidaJugador =
                objetivo.GetComponent<PlayerHealth>();
        }

        if (vidaJugador == null)
        {
            Debug.LogWarning(
                "El objetivo no tiene PlayerHealth."
            );

            return;
        }

        vidaJugador.RecibirDanio(
            data.dano
        );

        siguienteAtaque =
            Time.time +
            data.tiempoEntreAtaques;

        Debug.Log(
            "ENEMIGO ATACÓ AL JUGADOR | " +
            "Daño: " +
            data.dano
        );
    }

    // =========================================
    // ATAQUE AL OBJETIVO
    // =========================================

    private void AtacarObjetivo()
    {
        if (objetivo == null)
            return;

        ObjectiveHealth vidaObjetivo =
            objetivo.GetComponentInParent<ObjectiveHealth>();

        if (vidaObjetivo == null)
        {
            vidaObjetivo =
                objetivo.GetComponent<ObjectiveHealth>();
        }

        if (vidaObjetivo == null)
        {
            Debug.LogWarning(
                "El objeto con Tag Objetivo " +
                "no tiene ObjectiveHealth."
            );

            return;
        }

        vidaObjetivo.RecibirDanio(
            data.dano
        );

        siguienteAtaque =
            Time.time +
            data.tiempoEntreAtaques;

        Debug.Log(
            "ENEMIGO ATACÓ AL OBJETIVO | " +
            "Daño: " +
            data.dano +
            " | Vida restante: " +
            vidaObjetivo.VidaActual
        );
    }

    // =========================================
    // EMPUJE
    // =========================================

    public void RecibirEmpuje(
        Vector3 direccion,
        float fuerza)
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        direccion.Normalize();

        agent.Move(
            direccion *
            fuerza
        );
    }

    // =========================================
    // GIZMO
    // =========================================

    private void OnDrawGizmosSelected()
    {
        if (data == null)
            return;

        Gizmos.DrawWireSphere(
            transform.position,
            data.distanciaAtaque
        );
    }
}