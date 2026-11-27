using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class GrupoEnemigos
    {
        public GameObject enemigoPrefab;
        public int cantidad = 1;
        public float intervalo = 1f;
    }

    [System.Serializable]
    public class OleadaSpawner
    {
        public string nombre = "Oleada";
        public GrupoEnemigos[] grupos;
    }

    [System.Serializable]
    public class RutaAlternativa
    {
        public string nombre = "Ruta";
        public Transform[] puntos;
    }

    [Header("Rutas alternativas de este generador")]
    [SerializeField] private RutaAlternativa[] rutasAlternativas;

    [Header("Distribución de rutas")]
    [Range(0, 100)]
    [SerializeField] private int probabilidadRutaAlternativa = 50;

    [Header("Oleadas")]
    [SerializeField] private OleadaSpawner[] oleadas;

    [Header("Spawn")]
    [SerializeField] private Transform puntoSpawn;

    [Header("Separación")]
    [SerializeField] private float radioSpawn = 1f;

    [Header("Tutorial Nivel 1")]
    [SerializeField] private TutorialManager_Nivel tutorialManager;

    private WaveManager manager;

    public void IniciarOleada(
        int indiceOleada,
        WaveManager waveManager)
    {
        manager = waveManager;

        Debug.Log(
            gameObject.name +
            " recibió índice " +
            indiceOleada
        );

        StartCoroutine(
            GenerarOleada(indiceOleada)
        );
    }

    IEnumerator GenerarOleada(
        int indiceOleada)
    {
        // No tiene esta oleada configurada.
        if (oleadas == null ||
            indiceOleada < 0 ||
            indiceOleada >= oleadas.Length)
        {
            Debug.Log(
                gameObject.name +
                " NO tiene Element " +
                indiceOleada
            );

            manager.GeneradorTermino(indiceOleada);
            yield break;
        }

        OleadaSpawner oleada = oleadas[indiceOleada];

        // La oleada existe, pero no genera nada.
        if (oleada.grupos == null ||
            oleada.grupos.Length == 0)
        {
            Debug.Log(
                gameObject.name +
                " no genera nada en oleada " +
                (indiceOleada + 1)
            );

            manager.GeneradorTermino(indiceOleada);
            yield break;
        }

        Debug.Log(
            gameObject.name +
            " SÍ genera en oleada " +
            (indiceOleada + 1)
        );

        foreach (GrupoEnemigos grupo in oleada.grupos)
        {
            if (grupo == null || grupo.enemigoPrefab == null)
            {
                Debug.LogWarning(
                    gameObject.name +
                    " tiene un grupo sin prefab"
                );

                continue;
            }

            for (int i = 0; i < grupo.cantidad; i++)
            {
                CrearEnemigo(
                    grupo.enemigoPrefab,
                    indiceOleada
                );

                if (grupo.intervalo > 0f)
                {
                    yield return new WaitForSeconds(grupo.intervalo);
                }
            }
        }

        manager.GeneradorTermino(indiceOleada);
    }

    void CrearEnemigo(
        GameObject prefab,
        int indiceOleada)
    {
        Vector3 centro =
            puntoSpawn != null
            ? puntoSpawn.position
            : transform.position;

        Vector2 separacion =
            Random.insideUnitCircle * radioSpawn;

        Vector3 posicion =
            centro +
            new Vector3(
                separacion.x,
                0f,
                separacion.y
            );

        Quaternion rotacion =
            puntoSpawn != null
            ? puntoSpawn.rotation
            : transform.rotation;

        GameObject enemigo = Instantiate(
            prefab,
            posicion,
            rotacion
        );

        // Avisar al tutorial cuando aparece el primer enemigo.
        // El TutorialManager evita activar más de una vez el panel 9.
        if (tutorialManager != null)
        {
            tutorialManager.NotificarPrimerEnemigo();
        }

        AsignarRutaAleatoria(enemigo);

        manager.RegistrarEnemigo(indiceOleada);

        WaveEnemyTracker tracker =
            enemigo.GetComponent<WaveEnemyTracker>();

        if (tracker == null)
        {
            tracker = enemigo.AddComponent<WaveEnemyTracker>();
        }

        tracker.Configurar(
            manager,
            indiceOleada
        );

        Debug.Log(
            gameObject.name +
            " creó " +
            prefab.name +
            " | Oleada " +
            (indiceOleada + 1)
        );
    }

    private void AsignarRutaAleatoria(GameObject enemigo)
    {
        if (rutasAlternativas == null ||
            rutasAlternativas.Length == 0)
        {
            // Sin rutas configuradas, EnemyController conserva
            // su movimiento directo hacia el objetivo.
            return;
        }

        // Algunos enemigos conservan la ruta principal directa.
        // Solo los que superan esta probabilidad reciben una ruta alternativa.
        if (Random.Range(0, 100) >= probabilidadRutaAlternativa)
        {
            Debug.Log(
                enemigo.name +
                " usará la ruta principal directa del generador " +
                gameObject.name
            );
            return;
        }

        // Construir una lista de rutas válidas.
        System.Collections.Generic.List<RutaAlternativa> rutasValidas =
            new System.Collections.Generic.List<RutaAlternativa>();

        foreach (RutaAlternativa ruta in rutasAlternativas)
        {
            if (ruta != null &&
                ruta.puntos != null &&
                ruta.puntos.Length > 0)
            {
                rutasValidas.Add(ruta);
            }
        }

        if (rutasValidas.Count == 0)
        {
            Debug.LogWarning(
                gameObject.name +
                " no tiene rutas válidas configuradas."
            );
            return;
        }

        int indice = Random.Range(0, rutasValidas.Count);
        RutaAlternativa rutaElegida = rutasValidas[indice];

        EnemyController controlador =
            enemigo.GetComponent<EnemyController>();

        if (controlador == null)
        {
            Debug.LogWarning(
                enemigo.name +
                " no tiene EnemyController; no se pudo asignar una ruta."
            );
            return;
        }

        controlador.ConfigurarRuta(rutaElegida.puntos);

        Debug.Log(
            enemigo.name +
            " eligió la ruta '" +
            rutaElegida.nombre +
            "' del generador " +
            gameObject.name
        );
    }

    void OnDrawGizmosSelected()
    {
        Vector3 posicion =
            puntoSpawn != null
            ? puntoSpawn.position
            : transform.position;

        Gizmos.DrawWireSphere(
            posicion,
            radioSpawn
        );
    }
}
