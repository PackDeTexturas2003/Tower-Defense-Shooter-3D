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

    [Header("Oleadas")]
    [SerializeField] private OleadaSpawner[] oleadas;

    [Header("Spawn")]
    [SerializeField] private Transform puntoSpawn;

    [Header("Separación")]
    [SerializeField] private float radioSpawn = 1f;

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
        // No tiene esta oleada configurada
        if (indiceOleada < 0 ||
            indiceOleada >= oleadas.Length)
        {
            Debug.Log(
                gameObject.name +
                " NO tiene Element " +
                indiceOleada
            );

            manager.GeneradorTermino(
                indiceOleada
            );

            yield break;
        }

        OleadaSpawner oleada =
            oleadas[indiceOleada];

        // La oleada existe, pero no genera nada
        if (oleada.grupos == null ||
            oleada.grupos.Length == 0)
        {
            Debug.Log(
                gameObject.name +
                " no genera nada en oleada " +
                (indiceOleada + 1)
            );

            manager.GeneradorTermino(
                indiceOleada
            );

            yield break;
        }

        Debug.Log(
            gameObject.name +
            " SÍ genera en oleada " +
            (indiceOleada + 1)
        );

        foreach (GrupoEnemigos grupo in oleada.grupos)
        {
            if (grupo.enemigoPrefab == null)
            {
                Debug.LogWarning(
                    gameObject.name +
                    " tiene un grupo sin prefab"
                );

                continue;
            }

            for (int i = 0;
                 i < grupo.cantidad;
                 i++)
            {
                CrearEnemigo(
                    grupo.enemigoPrefab,
                    indiceOleada
                );

                if (grupo.intervalo > 0f)
                {
                    yield return new WaitForSeconds(
                        grupo.intervalo
                    );
                }
            }
        }

        manager.GeneradorTermino(
            indiceOleada
        );
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
            Random.insideUnitCircle *
            radioSpawn;

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

        GameObject enemigo =
            Instantiate(
                prefab,
                posicion,
                rotacion
            );

        manager.RegistrarEnemigo(
            indiceOleada
        );

        WaveEnemyTracker tracker =
            enemigo.GetComponent<
                WaveEnemyTracker>();

        if (tracker == null)
        {
            tracker =
                enemigo.AddComponent<
                    WaveEnemyTracker>();
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