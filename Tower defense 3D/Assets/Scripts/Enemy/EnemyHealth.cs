using System.Collections;
using UnityEngine;

public class EnemyHealth : Health
{
    [Header("Efecto al recibir daño")]
    [SerializeField] private Renderer[] renderersEnemigo;
    [SerializeField] private Color colorDanio = Color.red;
    [SerializeField] private float duracionColor = 0.15f;

    [Header("Efecto de muerte")]
    [SerializeField] private float duracionMuerte = 0.3f;

    [Header("Audio")]
    [SerializeField] private SFXManager sfxManager;

    [Header("Drops de Munición")]
    [SerializeField] private AmmoDrop[] dropsMunicion;

    [Header("Recompensa de dinero")]
    [SerializeField] private int dineroAlMorir = 50;

    private Color[] coloresOriginales;
    private Coroutine efectoDanio;

    private bool yaMurio = false;
    private int ultimoFrameSonido = -1;

    protected override void Awake()
    {
        base.Awake();

        if (renderersEnemigo == null ||
            renderersEnemigo.Length == 0)
        {
            renderersEnemigo =
                GetComponentsInChildren<Renderer>();
        }

        coloresOriginales =
            new Color[renderersEnemigo.Length];

        for (int i = 0;
             i < renderersEnemigo.Length;
             i++)
        {
            coloresOriginales[i] =
                renderersEnemigo[i].material.color;
        }

        if (sfxManager == null)
        {
            sfxManager =
                FindFirstObjectByType<SFXManager>();
        }
    }

    public override void RecibirDanio(float cantidad)
    {
        if (yaMurio)
            return;

        base.RecibirDanio(cantidad);

        if (ultimoFrameSonido != Time.frameCount)
        {
            ultimoFrameSonido = Time.frameCount;

            if (sfxManager != null)
            {
                sfxManager.ReproducirImpactoEnemigo();
            }
        }

        if (vidaActual > 0)
        {
            if (efectoDanio != null)
                StopCoroutine(efectoDanio);

            efectoDanio =
                StartCoroutine(EfectoDanio());
        }
    }

    private IEnumerator EfectoDanio()
    {
        for (int i = 0;
             i < renderersEnemigo.Length;
             i++)
        {
            if (renderersEnemigo[i] != null)
            {
                renderersEnemigo[i].material.color =
                    colorDanio;
            }
        }

        yield return new WaitForSeconds(
            duracionColor
        );

        for (int i = 0;
             i < renderersEnemigo.Length;
             i++)
        {
            if (renderersEnemigo[i] != null)
            {
                renderersEnemigo[i].material.color =
                    coloresOriginales[i];
            }
        }

        efectoDanio = null;
    }

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log(
            $"{gameObject.name} ha muerto."
        );

        DarRecompensaDinero();
        GenerarDropMunicion();

        EnemyController enemyController =
            GetComponent<EnemyController>();

        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (efectoDanio != null)
        {
            StopCoroutine(efectoDanio);
            efectoDanio = null;
        }

        StartCoroutine(
            AnimacionMuerte()
        );
    }

    private void DarRecompensaDinero()
    {
        if (dineroAlMorir <= 0)
            return;

        DineroJugador dineroJugador =
            FindFirstObjectByType<DineroJugador>();

        if (dineroJugador == null)
        {
            Debug.LogWarning(
                "EnemyHealth: No se encontró un DineroJugador en la escena."
            );

            return;
        }

        dineroJugador.AgregarDinero(
            dineroAlMorir
        );

        Debug.Log(
            "Recompensa por eliminar enemigo: +" +
            dineroAlMorir
        );
    }

    private IEnumerator AnimacionMuerte()
    {
        Vector3 escalaInicial =
            transform.localScale;

        float tiempo = 0f;

        while (tiempo < duracionMuerte)
        {
            tiempo += Time.deltaTime;

            float porcentaje =
                tiempo / duracionMuerte;

            transform.localScale =
                Vector3.Lerp(
                    escalaInicial,
                    Vector3.zero,
                    porcentaje
                );

            yield return null;
        }

        transform.localScale =
            Vector3.zero;

        Destroy(gameObject);
    }

    private void GenerarDropMunicion()
    {
        if (dropsMunicion == null ||
            dropsMunicion.Length == 0)
        {
            return;
        }

        float resultado =
            Random.Range(0f, 100f);

        float acumulado = 0f;

        for (int i = 0;
             i < dropsMunicion.Length;
             i++)
        {
            AmmoDrop drop =
                dropsMunicion[i];

            if (drop == null ||
                drop.prefab == null ||
                drop.probabilidad <= 0f)
            {
                continue;
            }

            acumulado +=
                drop.probabilidad;

            if (resultado <= acumulado)
            {
                Vector3 posicionDrop =
                    transform.position;

                Vector3 origenRaycast =
                    transform.position +
                    Vector3.up * 0.5f;

                if (Physics.Raycast(
                    origenRaycast,
                    Vector3.down,
                    out RaycastHit hit,
                    10f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    posicionDrop =
                        hit.point +
                        Vector3.up * 0.1f;
                }
                else
                {
                    posicionDrop +=
                        Vector3.down * 0.3f;
                }

                Instantiate(
                    drop.prefab,
                    posicionDrop,
                    Quaternion.identity
                );

                Debug.Log(
                    "Drop de munición generado: " +
                    drop.tipoMunicion
                );

                return;
            }
        }
    }
}

[System.Serializable]
public class AmmoDrop
{
    public AmmoType tipoMunicion;

    [Range(0f, 100f)]
    public float probabilidad = 20f;

    public GameObject prefab;
}