
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum TipoObjetivo
    {
        Jugador,
        Objetivo,
        Torreta
    }

    public enum TipoAtaque
    {
        CuerpoACuerpo,
        Distancia
    }

    [Header("Datos")]
    [SerializeField] private EnemyData data;

    // =========================================
    // ROTACION DEL MODELO VISUAL
    // =========================================

    [Header("Rotacion del modelo visual")]
    [SerializeField] private Transform modeloVisual;
    [SerializeField] private float rotacionYModelo = 0f;
    [SerializeField] private float velocidadRotacionModelo = 12f;

    private Vector3 rotacionBaseModelo;
    private Vector3 ultimaDireccionModelo = Vector3.forward;

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

    [Header("Deteccion de torretas")]
    [SerializeField]
    private string tagTorreta = "Torreta";

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
    // BUSQUEDA DE POSICION DE DISPARO
    // =========================================

    [Header("Buscar posicion de disparo")]
    [SerializeField] private LayerMask capasObstaculos;
    [SerializeField] private float distanciaBusqueda = 5f;
    [SerializeField] private int cantidadPosicionesBusqueda = 16;
    [SerializeField] private float alturaRaycast = 1.2f;
    [SerializeField] private float intervaloBusqueda = 0.5f;
    [SerializeField] private float distanciaLlegadaPosicion = 0.8f;

    private Vector3 posicionDisparoAlternativa;
    private bool tienePosicionAlternativa;
    private float siguienteBusqueda;

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

    // Torreta seleccionada por los enemigos de tipo Torreta.
    private Transform torretaObjetivoActual;

    private Transform objetivoActual;

    private CharacterController controller;

    private NavMeshAgent agent;

    // Ruta alternativa asignada por el spawner.
    private Transform[] puntosRuta;
    private int indicePuntoRuta;
    private const float distanciaLlegadaPuntoRuta = 0.75f;

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

        // Guardar la rotacion original del modelo.
        if (modeloVisual != null)
        {
            rotacionBaseModelo =
                modeloVisual.localEulerAngles;
        }
    }

    // =========================================
    // ROTACION DEL MODELO VISUAL
    // =========================================

    private void LateUpdate()
    {
        if (modeloVisual == null)
            return;

        // Obtener la direccion real del movimiento del enemigo.
        Vector3 direccionMovimiento = Vector3.zero;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            direccionMovimiento = agent.velocity;
        }

        // Ignorar el movimiento vertical.
        direccionMovimiento.y = 0f;

        // Actualizar la direccion cuando el enemigo se desplaza.
        if (direccionMovimiento.sqrMagnitude > 0.01f)
        {
            ultimaDireccionModelo =
                direccionMovimiento.normalized;
        }

        // Girar el modelo hacia la ultima direccion de movimiento.
        if (ultimaDireccionModelo.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(
                    ultimaDireccionModelo
                );

            // Aplicar la correccion de orientacion del modelo.
            rotacionObjetivo *= Quaternion.Euler(
                rotacionBaseModelo.x,
                rotacionBaseModelo.y + rotacionYModelo,
                rotacionBaseModelo.z
            );

            modeloVisual.rotation =
                Quaternion.Slerp(
                    modeloVisual.rotation,
                    rotacionObjetivo,
                    velocidadRotacionModelo *
                    Time.deltaTime
                );
        }
    }

    // =========================================
    // RUTA ASIGNADA POR EL SPAWNER
    // =========================================

    public void ConfigurarRuta(Transform[] nuevaRuta)
    {
        puntosRuta = nuevaRuta;
        indicePuntoRuta = 0;

        if (puntosRuta == null || puntosRuta.Length == 0)
        {
            Debug.LogWarning(
                gameObject.name + " recibi� una ruta vac�a."
            );
            puntosRuta = null;
            return;
        }

        Debug.Log(
            gameObject.name + " recibi� una ruta de " +
            puntosRuta.Length + " puntos."
        );
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
                " no est� colocado sobre el NavMesh."
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
                " encontr� al jugador."
            );
        }
    }

    // =========================================
    // BUSCAR OBJETIVO
    // =========================================

    private void BuscarObjetivo()
    {
        // El objetivo principal de respaldo es siempre el n�cleo.
        // Las torretas se buscan por separado para poder comprobar
        // si todav�a existen torretas activas.
        GameObject objetoEncontrado =
            GameObject.FindGameObjectWithTag("Objetivo");

        if (objetoEncontrado != null)
        {
            objetivoPrincipal =
                objetoEncontrado.transform;

            Debug.Log(
                gameObject.name +
                " encontr� al objetivo principal: " +
                objetoEncontrado.name
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
        // Enemigos que persiguen al jugador.
        if (tipoObjetivo == TipoObjetivo.Jugador)
        {
            objetivoActual = jugador;
            return;
        }

        // Enemigos que priorizan torretas,
        // pero atacan al jugador si se acerca.
        if (tipoObjetivo == TipoObjetivo.Torreta)
        {
            // PRIORIDAD 1: JUGADOR
            if (jugador != null && JugadorDentroDelRadio())
            {
                objetivoActual = jugador;
                return;
            }

            // PRIORIDAD 2: TORRETA
            torretaObjetivoActual = BuscarTorretaVivaMasCercana();

            if (torretaObjetivoActual != null)
            {
                objetivoActual = torretaObjetivoActual;
                return;
            }

            // PRIORIDAD 3: NÚCLEO
            objetivoActual = objetivoPrincipal;
            return;
        }

        // Enemigos que protegen el objetivo principal,
        // pero atacan al jugador si se acerca.
        if (jugador != null && JugadorDentroDelRadio())
        {
            objetivoActual = jugador;
            return;
        }

        objetivoActual = objetivoPrincipal;
    }

    // Busca la torreta activa m�s cercana al enemigo.
    // Los objetos inactivos no se consideran torretas vivas.
    private Transform BuscarTorretaVivaMasCercana()
    {
        GameObject[] torretas;

        try
        {
            torretas = GameObject.FindGameObjectsWithTag(tagTorreta);
        }
        catch (UnityException)
        {
            Debug.LogError(
                "El tag '" + tagTorreta +
                "' no est� definido en Unity."
            );

            return null;
        }

        Transform torretaMasCercana = null;
        float menorDistancia = Mathf.Infinity;

        foreach (GameObject torreta in torretas)
        {
            if (torreta == null || !torreta.activeInHierarchy)
                continue;

            float distancia = Vector3.Distance(
                transform.position,
                torreta.transform.position
            );

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                torretaMasCercana = torreta.transform;
            }
        }

        return torretaMasCercana;
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
        else if (EsTorreta(objetivoActual))
        {
            AtacarTorreta();
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
            "ENEMIGO ATAC� AL JUGADOR | " +
            "Da�o: " +
            dano
        );
    }

    // =========================================
    // DETECTAR TORRETA
    // =========================================

    private bool EsTorreta(Transform objetivoComprobar)
    {
        if (objetivoComprobar == null)
            return false;

        Transform actual = objetivoComprobar;

        while (actual != null)
        {
            if (actual.CompareTag(tagTorreta))
                return true;

            actual = actual.parent;
        }

        return false;
    }

    // =========================================
    // ATAQUE A LA TORRETA
    // =========================================

    private void AtacarTorreta()
    {
        if (objetivoActual == null)
            return;

        TurretHealth vidaTorreta =
            objetivoActual.GetComponentInParent<TurretHealth>();

        if (vidaTorreta == null)
        {
            vidaTorreta =
                objetivoActual.GetComponent<TurretHealth>();
        }

        if (vidaTorreta == null)
        {
            Debug.LogWarning(
                "La torreta no tiene TurretHealth."
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

        vidaTorreta.RecibirDanio(
            dano
        );

        siguienteAtaque =
            Time.time +
            tiempo;

        Debug.Log(
            "ENEMIGO ATAC� A LA TORRETA | " +
            "Da�o: " +
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
            "ENEMIGO ATAC� AL OBJETIVO | " +
            "Da�o: " +
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

        // La busqueda de posiciones alternativas se aplica al atacar torretas.
        // Contra jugador o nucleo se conserva el comportamiento habitual.
        if (!EsTorreta(objetivoActual))
        {
            tienePosicionAlternativa = false;

            float distanciaNormal = Vector3.Distance(
                transform.position,
                objetivoActual.position
            );

            if (distanciaNormal > radioDisparo)
            {
                PerseguirObjetivo();
                return;
            }

            DetenerMovimiento();
            MirarObjetivo();

            if (EstaMirandoAlObjetivo() && TieneLineaDeVision())
                DispararProyectil();

            return;
        }

        float distanciaTorreta = Vector3.Distance(
            transform.position,
            objetivoActual.position
        );

        bool visionDespejada = TieneLineaDeVision();

        // Si la torreta esta despejada y dentro del alcance, disparar.
        if (visionDespejada && distanciaTorreta <= radioDisparo)
        {
            tienePosicionAlternativa = false;
            DetenerMovimiento();
            MirarObjetivo();

            if (EstaMirandoAlObjetivo())
                DispararProyectil();

            return;
        }

        // Si no hay vision o esta fuera del alcance, buscar un punto
        // cercano, navegable y con linea de tiro libre.
        if (Time.time >= siguienteBusqueda)
        {
            siguienteBusqueda = Time.time + intervaloBusqueda;

            tienePosicionAlternativa = BuscarPosicionDeDisparo(
                out posicionDisparoAlternativa
            );
        }

        if (tienePosicionAlternativa)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                float distanciaAlPunto = Vector3.Distance(
                    transform.position,
                    posicionDisparoAlternativa
                );

                if (distanciaAlPunto > distanciaLlegadaPosicion)
                {
                    agent.isStopped = false;
                    agent.stoppingDistance = 0.2f;
                    agent.SetDestination(posicionDisparoAlternativa);
                }
                else
                {
                    // Al llegar, validar desde el punto real de disparo.
                    if (TieneLineaDeVision() &&
                        Vector3.Distance(transform.position, objetivoActual.position) <= radioDisparo)
                    {
                        DetenerMovimiento();
                        MirarObjetivo();

                        if (EstaMirandoAlObjetivo())
                            DispararProyectil();
                    }
                    else
                    {
                        tienePosicionAlternativa = false;
                    }
                }
            }
        }
        else
        {
            // Si no encuentra un punto valido, avanzar hacia la torreta
            // y volver a buscar en el siguiente intervalo.
            PerseguirObjetivo();
        }
    }

    // =========================================
    // COMPROBAR LINEA DE VISION
    // =========================================

    private bool TieneLineaDeVision()
    {
        if (objetivoActual == null || puntoDisparo == null)
            return false;

        Vector3 origen = puntoDisparo.position;
        Vector3 destino = objetivoActual.position + Vector3.up * 0.8f;
        Vector3 direccion = destino - origen;
        float distancia = direccion.magnitude;

        if (distancia <= 0.01f)
            return true;

        return !Physics.Raycast(
            origen,
            direccion.normalized,
            distancia,
            capasObstaculos,
            QueryTriggerInteraction.Ignore
        );
    }

    // =========================================
    // BUSCAR POSICION DE DISPARO
    // =========================================

    private bool BuscarPosicionDeDisparo(out Vector3 posicionEncontrada)
    {
        posicionEncontrada = transform.position;

        if (objetivoActual == null || agent == null || !agent.isOnNavMesh)
            return false;

        int cantidad = Mathf.Max(4, cantidadPosicionesBusqueda);
        float radio = Mathf.Clamp(distanciaBusqueda, 1f, radioDisparo);

        // Probar posiciones alrededor de la torreta.
        for (int i = 0; i < cantidad; i++)
        {
            float angulo = i * (360f / cantidad) * Mathf.Deg2Rad;
            Vector3 direccion = new Vector3(
                Mathf.Cos(angulo),
                0f,
                Mathf.Sin(angulo)
            );

            Vector3 candidata = objetivoActual.position + direccion * radio;
            NavMeshHit hit;

            if (!NavMesh.SamplePosition(
                candidata,
                out hit,
                2f,
                NavMesh.AllAreas))
                continue;

            float distanciaObjetivo = Vector3.Distance(
                hit.position,
                objetivoActual.position
            );

            if (distanciaObjetivo > radioDisparo)
                continue;

            // Confirmar que el agente realmente puede llegar al punto.
            NavMeshPath ruta = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, ruta) ||
                ruta.status != NavMeshPathStatus.PathComplete)
                continue;

            // Simular el punto de disparo a la altura del arma.
            Vector3 origen = hit.position + Vector3.up * alturaRaycast;
            Vector3 destino = objetivoActual.position + Vector3.up * 0.8f;
            Vector3 direccionVision = destino - origen;
            float distanciaVision = direccionVision.magnitude;

            if (distanciaVision <= 0.01f)
                continue;

            bool bloqueado = Physics.Raycast(
                origen,
                direccionVision.normalized,
                distanciaVision,
                capasObstaculos,
                QueryTriggerInteraction.Ignore
            );

            if (bloqueado)
                continue;

            posicionEncontrada = hit.position;

            Debug.Log(
                gameObject.name +
                " encontro una posicion despejada para atacar a " +
                objetivoActual.name
            );

            return true;
        }

        Debug.Log(
            gameObject.name +
            " no encontro una posicion despejada para atacar a " +
            objetivoActual.name
        );

        return false;
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
            " DISPAR� A " +
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

        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        // Distancia normal para atacar al objetivo final.
        float distanciaParadaObjetivo =
            tipoAtaque == TipoAtaque.CuerpoACuerpo
            ? (data != null ? data.distanciaAtaque : 1.5f)
            : radioDisparo;

        Vector3 destinoMovimiento = objetivoActual.position;

        // Las rutas alternativas solo se usan cuando el enemigo
        // se dirige al objetivo principal, no al jugador ni a torretas.
        if (objetivoActual == objetivoPrincipal &&
            puntosRuta != null &&
            indicePuntoRuta < puntosRuta.Length)
        {
            // IMPORTANTE: reducir la distancia de parada ANTES de
            // comprobar la llegada. Si se usa la distancia de ataque
            // (por ejemplo 1.5), el agente puede detenerse lejos del punto.
            agent.stoppingDistance = 0.3f;

            // Saltar puntos que hayan sido eliminados o est�n vac�os.
            while (indicePuntoRuta < puntosRuta.Length &&
                   puntosRuta[indicePuntoRuta] == null)
            {
                indicePuntoRuta++;
            }

            // Si ya hay una ruta calculada y el agente lleg� al punto,
            // avanzar al siguiente.
            if (indicePuntoRuta < puntosRuta.Length &&
                !agent.pathPending &&
                agent.hasPath &&
                agent.remainingDistance <= 0.45f)
            {
                indicePuntoRuta++;

                // Saltar posibles referencias vac�as siguientes.
                while (indicePuntoRuta < puntosRuta.Length &&
                       puntosRuta[indicePuntoRuta] == null)
                {
                    indicePuntoRuta++;
                }
            }

            // Si quedan puntos, dirigirse al siguiente.
            if (indicePuntoRuta < puntosRuta.Length)
            {
                destinoMovimiento = puntosRuta[indicePuntoRuta].position;
            }
            else
            {
                // Termin� la ruta alternativa: continuar al objetivo
                // con la distancia de parada habitual del enemigo.
                agent.stoppingDistance = distanciaParadaObjetivo;
            }
        }
        else
        {
            // Persecuci�n normal del jugador, torretas u objetivo
            // cuando no hay ruta alternativa activa.
            agent.stoppingDistance = distanciaParadaObjetivo;
        }

        agent.SetDestination(destinoMovimiento);
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

        // Amarillo = detecci�n del jugador.
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
