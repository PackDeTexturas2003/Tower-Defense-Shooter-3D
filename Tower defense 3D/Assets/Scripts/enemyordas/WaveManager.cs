using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Oleada
    {
        public string nombre = "Oleada";

        [Header("Tiempo de la horda")]
        public float tiempoLimite = 30f;

        [Header("Tiempo antes de la siguiente horda")]
        public float esperaSiguienteOleada = 3f;
    }

    [Header("Spawners")]
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Oleadas")]
    [SerializeField] private Oleada[] oleadas;

    [Header("Inicio")]
    [SerializeField] private float esperaPrimeraOleada = 2f;

    private int oleadaActual = -1;

    private int spawnersTerminados;

    // Enemigos que deben eliminarse para considerar
    // completada la horda actual.
    private int enemigosGeneradosActual;

    private int enemigosMuertosActual;

    // Todos los enemigos vivos del escenario,
    // incluyendo sobrevivientes de hordas anteriores.
    private int enemigosVivosTotal;

    private float tiempoInicio;
    private float tiempoRestante;

    private bool oleadaActiva;
    private bool esperandoSiguiente;
    private bool todasLasOleadasGeneradas;
    private bool juegoGanado;

    // =========================================
    // PROPIEDADES
    // =========================================

    public int OleadaActual
    {
        get
        {
            if (TotalOleadas <= 0)
                return 0;

            return Mathf.Clamp(
                oleadaActual + 1,
                1,
                TotalOleadas
            );
        }
    }

    public int TotalOleadas
    {
        get
        {
            if (oleadas == null)
                return 0;

            return oleadas.Length;
        }
    }

    public int EnemigosRestantes
    {
        get
        {
            // Ahora muestra TODOS los enemigos vivos,
            // incluyendo sobrevivientes de hordas anteriores.
            return Mathf.Max(
                0,
                enemigosVivosTotal
            );
        }
    }

    public int EnemigosTotales
    {
        get
        {
            return enemigosGeneradosActual;
        }
    }

    public int EnemigosVivosTotal
    {
        get
        {
            return enemigosVivosTotal;
        }
    }

    public float TiempoRestante
    {
        get
        {
            return tiempoRestante;
        }
    }

    public bool OleadaActiva
    {
        get
        {
            return oleadaActiva;
        }
    }

    public bool TodasLasOleadasTerminadas
    {
        get
        {
            return todasLasOleadasGeneradas &&
                   enemigosVivosTotal <= 0;
        }
    }

    public bool JuegoGanado
    {
        get
        {
            return juegoGanado;
        }
    }

    // =========================================
    // INICIO
    // =========================================

    private void Start()
    {
        StartCoroutine(
            Inicio()
        );
    }

    private void Update()
    {
        if (!oleadaActiva)
            return;

        tiempoRestante -=
            Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;

            TiempoAgotado();
        }
    }

    private IEnumerator Inicio()
    {
        yield return new WaitForSeconds(
            esperaPrimeraOleada
        );

        if (TotalOleadas <= 0)
        {
            Debug.LogWarning(
                "No hay hordas configuradas."
            );

            yield break;
        }

        IniciarSiguienteOleada();
    }

    // =========================================
    // NUEVA HORDA
    // =========================================

    private void IniciarSiguienteOleada()
    {
        esperandoSiguiente = false;

        if (oleadaActual + 1 >= TotalOleadas)
        {
            todasLasOleadasGeneradas = true;

            oleadaActiva = false;

            Debug.Log(
                "================================"
            );

            Debug.Log(
                "TODAS LAS HORDAS FUERON GENERADAS"
            );

            Debug.Log(
                "Enemigos vivos restantes: " +
                enemigosVivosTotal
            );

            Debug.Log(
                "================================"
            );

            ComprobarFinalDelJuego();

            return;
        }

        oleadaActual++;

        Oleada datos =
            oleadas[oleadaActual];

        spawnersTerminados = 0;

        // =====================================
        // IMPORTANTE
        // =====================================
        //
        // Los enemigos que sobrevivieron de la
        // horda anterior pasan a formar parte
        // de la nueva horda.
        //
        // Ejemplo:
        //
        // Horda 1:
        // 5 enemigos
        // queda 1
        //
        // Horda 2:
        // empieza con 1 enemigo
        // + enemigos nuevos
        //
        enemigosGeneradosActual =
            enemigosVivosTotal;

        enemigosMuertosActual = 0;

        tiempoInicio =
            Time.time;

        tiempoRestante =
            datos.tiempoLimite;

        oleadaActiva = true;

        Debug.Log(
            "=============================="
        );

        Debug.Log(
            "INICIA HORDA " +
            (oleadaActual + 1) +
            " / " +
            TotalOleadas
        );

        Debug.Log(
            "Nombre: " +
            datos.nombre
        );

        Debug.Log(
            "Tiempo límite: " +
            datos.tiempoLimite +
            " segundos"
        );

        Debug.Log(
            "Enemigos sobrevivientes " +
            "de hordas anteriores: " +
            enemigosVivosTotal
        );

        Debug.Log(
            "Spawners registrados: " +
            spawners.Length
        );

        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner == null)
            {
                spawnersTerminados++;

                continue;
            }

            Debug.Log(
                "Mandando horda " +
                (oleadaActual + 1) +
                " a " +
                spawner.gameObject.name
            );

            spawner.IniciarOleada(
                oleadaActual,
                this
            );
        }

        RevisarOleada();
    }

    // =========================================
    // REGISTRAR ENEMIGO
    // =========================================

    public void RegistrarEnemigo(
        int indiceOleada)
    {
        enemigosVivosTotal++;

        // Todo enemigo nuevo que aparece durante
        // la horda actual se suma a la cantidad
        // que debe ser eliminada.
        if (indiceOleada == oleadaActual)
        {
            enemigosGeneradosActual++;
        }

        Debug.Log(
            "Enemigo generado | " +
            "Horda: " +
            (indiceOleada + 1) +
            " / " +
            TotalOleadas +
            " | Enemigos vivos totales: " +
            enemigosVivosTotal
        );
    }

    // =========================================
    // REGISTRAR MUERTE
    // =========================================

    public void RegistrarMuerteEnemigo(
        int indiceOleada)
    {
        enemigosVivosTotal--;

        if (enemigosVivosTotal < 0)
        {
            enemigosVivosTotal = 0;
        }

        // =====================================
        // IMPORTANTE
        // =====================================
        //
        // Ya no comprobamos que el enemigo
        // pertenezca a la oleada actual.
        //
        // Esto permite que un sobreviviente de
        // la horda anterior cuente al morir.
        //
        if (oleadaActiva)
        {
            enemigosMuertosActual++;

            Debug.Log(
                "Enemigo eliminado durante " +
                "la horda actual | " +
                "Eliminados: " +
                enemigosMuertosActual +
                "/" +
                enemigosGeneradosActual
            );

            RevisarOleada();
        }

        Debug.Log(
            "Enemigo murió | " +
            "Horda original: " +
            (indiceOleada + 1) +
            " / " +
            TotalOleadas +
            " | " +
            "Enemigos vivos totales: " +
            enemigosVivosTotal
        );

        ComprobarFinalDelJuego();
    }

    // =========================================
    // SPAWNER TERMINÓ
    // =========================================

    public void GeneradorTermino(
        int indiceOleada)
    {
        if (indiceOleada != oleadaActual)
            return;

        spawnersTerminados++;

        Debug.Log(
            "Spawner terminó | " +
            spawnersTerminados +
            "/" +
            spawners.Length
        );

        RevisarOleada();
    }

    // =========================================
    // REVISAR HORDA
    // =========================================

    private void RevisarOleada()
    {
        if (!oleadaActiva)
            return;

        if (esperandoSiguiente)
            return;

        if (spawnersTerminados < spawners.Length)
            return;

        // Si no existen enemigos vivos y no se
        // generaron enemigos en esta horda,
        // la horda se considera completada.
        if (enemigosGeneradosActual == 0)
        {
            Debug.Log(
                "Horda " +
                (oleadaActual + 1) +
                " / " +
                TotalOleadas +
                " no generó enemigos."
            );

            OleadaCompletada();

            return;
        }

        if (enemigosMuertosActual >=
            enemigosGeneradosActual)
        {
            OleadaCompletada();
        }
    }

    // =========================================
    // HORDA COMPLETADA
    // =========================================

    private void OleadaCompletada()
    {
        if (!oleadaActiva)
            return;

        oleadaActiva = false;

        float tiempoUsado =
            Time.time -
            tiempoInicio;

        Debug.Log(
            "LIMPIASTE LA HORDA " +
            (oleadaActual + 1) +
            " / " +
            TotalOleadas +
            " EN " +
            tiempoUsado.ToString("F1") +
            " SEGUNDOS"
        );

        float espera =
            oleadas[oleadaActual]
                .esperaSiguienteOleada;

        Debug.Log(
            "La siguiente horda comenzará en " +
            espera +
            " segundos."
        );

        PrepararSiguiente(
            espera
        );
    }

    // =========================================
    // TIEMPO AGOTADO
    // =========================================

    private void TiempoAgotado()
    {
        if (!oleadaActiva)
            return;

        oleadaActiva = false;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "TIEMPO AGOTADO - HORDA " +
            (oleadaActual + 1) +
            " / " +
            TotalOleadas
        );

        Debug.Log(
            "Enemigos vivos que pasan " +
            "a la siguiente horda: " +
            enemigosVivosTotal
        );

        Debug.Log(
            "================================"
        );

        float espera =
            oleadas[oleadaActual]
                .esperaSiguienteOleada;

        PrepararSiguiente(
            espera
        );
    }

    // =========================================
    // ESPERAR SIGUIENTE HORDA
    // =========================================

    private void PrepararSiguiente(
        float tiempo)
    {
        if (esperandoSiguiente)
            return;

        esperandoSiguiente = true;

        StartCoroutine(
            EsperarSiguiente(
                tiempo
            )
        );
    }

    private IEnumerator EsperarSiguiente(
        float tiempo)
    {
        yield return new WaitForSeconds(
            tiempo
        );

        // Los enemigos sobrevivientes permanecen
        // vivos y pasan a la siguiente horda.
        IniciarSiguienteOleada();
    }

    // =========================================
    // FINAL DEL JUEGO
    // =========================================

    private void ComprobarFinalDelJuego()
    {
        if (juegoGanado)
            return;

        if (!todasLasOleadasGeneradas)
            return;

        if (enemigosVivosTotal > 0)
            return;

        juegoGanado = true;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "¡¡¡ GANASTE !!!"
        );

        Debug.Log(
            "TODAS LAS HORDAS COMPLETADAS"
        );

        Debug.Log(
            "NO QUEDAN ENEMIGOS VIVOS"
        );

        Debug.Log(
            "================================"
        );
    }
}