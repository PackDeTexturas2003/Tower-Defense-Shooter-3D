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

    private int enemigosGeneradosActual;

    private int enemigosMuertosActual;

    private int enemigosVivosTotal;

    private float tiempoInicio;
    private float tiempoRestante;

    private bool oleadaActiva;
    private bool esperandoSiguiente;
    private bool todasLasOleadasGeneradas;
    private bool juegoGanado;

    private bool juegoIniciado;

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

    private void Update()
    {
        if (!juegoIniciado)
            return;

        if (!oleadaActiva)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;

            TiempoAgotado();
        }
    }

    public void IniciarJuego()
    {
        if (juegoIniciado)
            return;

        juegoIniciado = true;

        Debug.Log("================================");
        Debug.Log("WAVE MANAGER INICIADO");
        Debug.Log("Esperando " + esperaPrimeraOleada + " segundos para la primera horda.");
        Debug.Log("================================");

        StartCoroutine(
            Inicio()
        );
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

    public void RegistrarEnemigo(
        int indiceOleada)
    {
        enemigosVivosTotal++;

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

    public void RegistrarMuerteEnemigo(
        int indiceOleada)
    {
        enemigosVivosTotal--;

        if (enemigosVivosTotal < 0)
        {
            enemigosVivosTotal = 0;
        }

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

    private void RevisarOleada()
    {
        if (!oleadaActiva)
            return;

        if (esperandoSiguiente)
            return;

        if (spawnersTerminados < spawners.Length)
            return;

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

        IniciarSiguienteOleada();
    }

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