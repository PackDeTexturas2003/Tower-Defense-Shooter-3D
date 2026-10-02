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

    public enum TipoAtaque
    {
        CuerpoACuerpo,
        Distancia
    }

    [Header("Datos")]
    [SerializeField] private EnemyData data;

    [Header("Tipo de ataque")]
    [SerializeField]
    private TipoAtaque tipoAtaque =
        TipoAtaque.CuerpoACuerpo;

    [Header("Tipo de objetivo")]
    [SerializeField]
    private TipoObjetivo tipoObjetivo =
        TipoObjetivo.Jugador;

    [Header("Objetivo")]
    [SerializeField] private Transform objetivo;

    // =========================================
    // DETECCION
    // =========================================

    [Header("Deteccion del jugador")]
    [SerializeField]
    private float radioDeteccionJugador = 12f;

    // =========================================
    // ATAQUE A DISTANCIA
    // =========================================

    [Header("Ataque a distancia")]

    [SerializeField]
    private GameObject proyectilPrefab;

    [SerializeField]
    private Transform puntoDisparo;

    [SerializeField]
    private float radioDisparo = 10f;

    [SerializeField]
    private float velocidadProyectil = 18f;

    [SerializeField]
    private float danoProyectil = 10f;

    [SerializeField]
    private float tiempoEntreDisparos = 2f;

    // =========================================
    // APUNTADO
    // =========================================

    [Header("Apuntado")]

    [SerializeField]
    private float velocidadRotacion = 8f;

    [SerializeField]
    [Range(0f, 1f)]
    private float precisionParaDisparar = 0.90f;

    // =========================================
    // PROYECTIL
    // =========================================

    [Header("Proyectil")]

    [SerializeField]
    private float tiempoVidaProyectil = 5f;

    // =========================================
    // REFERENCIAS
    // =========================================

    private Transform jugador;

    private Transform objetivoPrincipal;

    private Transform objetivoActual;

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

        // El movimiento lo controla
        // NavMeshAgent.
        controller.enabled = false;
    }

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        ConfigurarAgente();

        BuscarJugador();

        BuscarObjetivo();

        ConfigurarObjetivoPrincipal();
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

        agent.angularSpeed = 360f;

        agent.acceleration = 20f;

        if (data != null)
        {
            if (tipoAtaque ==
                TipoAtaque.CuerpoACuerpo)
            {
                agent.stoppingDistance =
                    data.distanciaAtaque;
            }
            else
            {
                agent.stoppingDistance =
                    radioDisparo;
            }
        }
        else
        {
            agent.stoppingDistance =
                tipoAtaque ==
                TipoAtaque.CuerpoACuerpo
                ? 1.5f
                : radioDisparo;
        }

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
        if (!agentePreparado)
        {
            if (agent != null &&
                agent.isOnNavMesh)
            {
                ConfigurarAgente();
            }

            return;
        }

        BuscarReferenciasSiEsNecesario();

        ActualizarObjetivoActual();

        if (objetivoActual == null)
            return;

        if (tipoAtaque ==
            TipoAtaque.CuerpoACuerpo)
        {
            ComportamientoCuerpoACuerpo();
        }
        else
        {
            ComportamientoDistancia();
        }
    }

    // =========================================
    // BUSCAR REFERENCIAS
    // =========================================

    private void BuscarReferenciasSiEsNecesario()
    {
        if (jugador == null)
            BuscarJugador();

        if (objetivoPrincipal == null)
            BuscarObjetivo();
    }

    // =========================================
    // BUSCAR JUGADOR
    // =========================================

    private void BuscarJugador()
    {
        GameObject objetoJugador =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (objetoJugador != null)
        {
            jugador =
                objetoJugador.transform;

            Debug.Log(
                gameObject.name +
                " encontró al jugador."
            );
        }
    }

    // =========================================
    // BUSCAR OBJETIVO
    // =========================================

    private void BuscarObjetivo()
    {
        GameObject objetoEncontrado =
            GameObject.FindGameObjectWithTag(
                "Objetivo"
            );

        if (objetoEncontrado != null)
        {
            objetivoPrincipal =
                objetoEncontrado.transform;

            Debug.Log(
                gameObject.name +
                " encontró al objetivo."
            );
        }
    }

    // =========================================
    // CONFIGURAR OBJETIVO PRINCIPAL
    // =========================================

    private void ConfigurarObjetivoPrincipal()
    {
        if (tipoObjetivo ==
            TipoObjetivo.Jugador)
        {
            objetivoActual =
                jugador;
        }
        else
        {
            objetivoActual =
                objetivoPrincipal;
        }
    }

    // =========================================
    // ACTUALIZAR OBJETIVO
    // =========================================

    private void ActualizarObjetivoActual()
    {
        // Si el enemigo tiene como objetivo
        // principal al jugador.
        if (tipoObjetivo ==
            TipoObjetivo.Jugador)
        {
            objetivoActual =
                jugador;

            return;
        }

        // Si el enemigo tiene como objetivo
        // principal el núcleo/objetivo,
        // pero el jugador entra en su radio,
        // empieza a perseguir al jugador.
        if (jugador != null &&
            JugadorDentroDelRadio())
        {
            objetivoActual =
                jugador;

            return;
        }

        // Si el jugador está fuera del radio,
        // vuelve al objetivo principal.
        objetivoActual =
            objetivoPrincipal;
    }

    // =========================================
    // RADIO DE DETECCION
    // =========================================

    private bool JugadorDentroDelRadio()
    {
        if (jugador == null)
            return false;

        Vector3 posicionEnemigo =
            transform.position;

        Vector3 posicionJugador =
            jugador.position;

        posicionEnemigo.y = 0f;

        posicionJugador.y = 0f;

        float distancia =
            Vector3.Distance(
                posicionEnemigo,
                posicionJugador
            );

        return distancia <=
               radioDeteccionJugador;
    }

    // =========================================
    // CUERPO A CUERPO
    // =========================================

    private void ComportamientoCuerpoACuerpo()
    {
        if (EnRangoDeAtaque())
        {
            DetenerMovimiento();

            MirarObjetivo();

            Atacar();

            return;
        }

        PerseguirObjetivo();
    }

    // =========================================
    // COMPROBAR RANGO DE ATAQUE
    // =========================================

    private bool EnRangoDeAtaque()
    {
        if (objetivoActual == null)
            return false;

        Vector3 posicionEnemigo =
            transform.position;

        Vector3 posicionObjetivo =
            objetivoActual.position;

        posicionEnemigo.y = 0f;

        posicionObjetivo.y = 0f;

        float distancia =
            Vector3.Distance(
                posicionEnemigo,
                posicionObjetivo
            );

        float distanciaAtaque =
            data != null
            ? data.distanciaAtaque
            : 1.5f;

        return distancia <=
               distanciaAtaque;
    }

    // =========================================
    // ATAQUE
    // =========================================

    private void Atacar()
    {
        if (Time.time < siguienteAtaque)
            return;

        if (objetivoActual == jugador)
        {
            AtacarJugador();
        }
        else
        {
            AtacarObjetivo();
        }
    }

    // =========================================
    // ATAQUE AL JUGADOR
    // =========================================

    private void AtacarJugador()
    {
        if (jugador == null)
            return;

        PlayerHealth vidaJugador =
            jugador.GetComponentInParent<PlayerHealth>();

        if (vidaJugador == null)
        {
            vidaJugador =
                jugador.GetComponent<PlayerHealth>();
        }

        if (vidaJugador == null)
        {
            Debug.LogWarning(
                "El jugador no tiene PlayerHealth."
            );

            return;
        }

        float dano =
            data != null
            ? data.dano
            : 10f;

        float tiempo =
            data != null
            ? data.tiempoEntreAtaques
            : 1f;

        vidaJugador.RecibirDanio(
            dano
        );

        siguienteAtaque =
            Time.time +
            tiempo;

        Debug.Log(
            "ENEMIGO ATACÓ AL JUGADOR | " +
            "Daño: " +
            dano
        );
    }

    // =========================================
    // ATAQUE AL OBJETIVO
    // =========================================

    private void AtacarObjetivo()
    {
        if (objetivoActual == null)
            return;

        ObjectiveHealth vidaObjetivo =
            objetivoActual.GetComponentInParent<ObjectiveHealth>();

        if (vidaObjetivo == null)
        {
            vidaObjetivo =
                objetivoActual.GetComponent<ObjectiveHealth>();
        }

        if (vidaObjetivo == null)
        {
            Debug.LogWarning(
                "El objeto con Tag Objetivo " +
                "no tiene ObjectiveHealth."
            );

            return;
        }

        float dano =
            data != null
            ? data.dano
            : 10f;

        float tiempo =
            data != null
            ? data.tiempoEntreAtaques
            : 1f;

        vidaObjetivo.RecibirDanio(
            dano
        );

        siguienteAtaque =
            Time.time +
            tiempo;

        Debug.Log(
            "ENEMIGO ATACÓ AL OBJETIVO | " +
            "Daño: " +
            dano +
            " | Vida restante: " +
            vidaObjetivo.VidaActual
        );
    }

    // =========================================
    // ATAQUE A DISTANCIA
    // =========================================

    private void ComportamientoDistancia()
    {
        if (objetivoActual == null)
            return;

        float distancia =
            Vector3.Distance(
                transform.position,
                objetivoActual.position
            );

        // Fuera del radio de disparo:
        // acercarse.
        if (distancia > radioDisparo)
        {
            PerseguirObjetivo();

            return;
        }

        // Dentro del radio:
        // detenerse.
        DetenerMovimiento();

        // Girar hacia el objetivo.
        MirarObjetivo();

        // Cuando está correctamente apuntado,
        // disparar.
        if (EstaMirandoAlObjetivo())
        {
            DispararProyectil();
        }
    }

    // =========================================
    // DISPARAR PROYECTIL
    // =========================================

    private void DispararProyectil()
    {
        if (Time.time < siguienteAtaque)
            return;

        if (proyectilPrefab == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " no tiene Proyectil Prefab asignado."
            );

            return;
        }

        if (puntoDisparo == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " no tiene Punto Disparo asignado."
            );

            return;
        }

        Vector3 direccion =
            objetivoActual.position -
            puntoDisparo.position;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        direccion.Normalize();

        GameObject proyectil =
            Instantiate(
                proyectilPrefab,
                puntoDisparo.position,
                Quaternion.LookRotation(
                    direccion
                )
            );

        EnemyProjectile enemyProjectile =
            proyectil.GetComponent<EnemyProjectile>();

        if (enemyProjectile == null)
        {
            Debug.LogError(
                "El prefab del proyectil no tiene " +
                "el componente EnemyProjectile."
            );

            Destroy(proyectil);

            return;
        }

        enemyProjectile.Configurar(
            direccion,
            velocidadProyectil,
            danoProyectil,
            tiempoVidaProyectil
        );

        siguienteAtaque =
            Time.time +
            tiempoEntreDisparos;

        Debug.Log(
            gameObject.name +
            " DISPARÓ A " +
            objetivoActual.name
        );
    }

    // =========================================
    // COMPROBAR APUNTADO
    // =========================================

    private bool EstaMirandoAlObjetivo()
    {
        if (objetivoActual == null)
            return false;

        Vector3 direccion =
            objetivoActual.position -
            transform.position;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return true;

        direccion.Normalize();

        float productoPunto =
            Vector3.Dot(
                transform.forward,
                direccion
            );

        return productoPunto >=
               precisionParaDisparar;
    }

    // =========================================
    // PERSEGUIR
    // =========================================

    private void PerseguirObjetivo()
    {
        if (objetivoActual == null)
            return;

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        if (tipoAtaque ==
            TipoAtaque.CuerpoACuerpo)
        {
            agent.stoppingDistance =
                data != null
                ? data.distanciaAtaque
                : 1.5f;
        }
        else
        {
            agent.stoppingDistance =
                radioDisparo;
        }

        agent.SetDestination(
            objetivoActual.position
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
        if (objetivoActual == null)
            return;

        Vector3 direccion =
            objetivoActual.position -
            transform.position;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        direccion.Normalize();

        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(
                direccion
            );

        float velocidad =
            tipoAtaque ==
            TipoAtaque.Distancia
            ? velocidadRotacion
            : 10f;

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidad *
                Time.deltaTime
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
    // GIZMOS
    // =========================================

    private void OnDrawGizmosSelected()
    {
        // Rojo = ataque cuerpo a cuerpo.
        if (data != null)
        {
            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                transform.position,
                data.distanciaAtaque
            );
        }

        // Amarillo = detección del jugador.
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            radioDeteccionJugador
        );

        // Azul = radio de disparo.
        if (tipoAtaque ==
            TipoAtaque.Distancia)
        {
            Gizmos.color =
                Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                radioDisparo
            );
        }

        // Cian = punto de disparo.
        if (puntoDisparo != null)
        {
            Gizmos.color =
                Color.cyan;

            Gizmos.DrawSphere(
                puntoDisparo.position,
                0.08f
            );

            Gizmos.DrawLine(
                puntoDisparo.position,
                puntoDisparo.position +
                puntoDisparo.forward
            );
        }
    }
}