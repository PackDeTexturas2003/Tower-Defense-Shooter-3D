
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
    private bool yaMurio;
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

        coloresOriginales = new Color[renderersEnemigo.Length];

        for (int i = 0; i < renderersEnemigo.Length; i++)
        {
            if (renderersEnemigo[i] != null)
                coloresOriginales[i] =
                    renderersEnemigo[i].material.color;
        }

        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();
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
                sfxManager.ReproducirImpactoEnemigo();
        }

        if (vidaActual > 0)
        {
            if (efectoDanio != null)
                StopCoroutine(efectoDanio);

            efectoDanio = StartCoroutine(EfectoDanio());
        }
    }

    private IEnumerator EfectoDanio()
    {
        for (int i = 0; i < renderersEnemigo.Length; i++)
        {
            if (renderersEnemigo[i] != null)
                renderersEnemigo[i].material.color = colorDanio;
        }

        yield return new WaitForSeconds(duracionColor);

        for (int i = 0; i < renderersEnemigo.Length; i++)
        {
            if (renderersEnemigo[i] != null)
                renderersEnemigo[i].material.color =
                    coloresOriginales[i];
        }

        efectoDanio = null;
    }

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log(gameObject.name + " ha muerto.");

        DarRecompensaDinero();
        GenerarDropMunicion();

        EnemyController enemyController =
            GetComponent<EnemyController>();

        if (enemyController != null)
            enemyController.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (efectoDanio != null)
        {
            StopCoroutine(efectoDanio);
            efectoDanio = null;
        }

        StartCoroutine(AnimacionMuerte());
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
                "EnemyHealth: No se encontró DineroJugador."
            );
            return;
        }

        dineroJugador.AgregarDinero(dineroAlMorir);

        Debug.Log(
            "Recompensa por eliminar enemigo: +" + dineroAlMorir
        );
    }

    private IEnumerator AnimacionMuerte()
    {
        Vector3 escalaInicial = transform.localScale;
        float tiempo = 0f;

        while (tiempo < duracionMuerte)
        {
            tiempo += Time.deltaTime;

            float porcentaje = duracionMuerte > 0f
                ? Mathf.Clamp01(tiempo / duracionMuerte)
                : 1f;

            transform.localScale = Vector3.Lerp(
                escalaInicial,
                Vector3.zero,
                porcentaje
            );

            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    private void GenerarDropMunicion()
    {
        if (dropsMunicion == null || dropsMunicion.Length == 0)
            return;

        WeaponManager weaponManager =
            FindFirstObjectByType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogWarning(
                "EnemyHealth: No se encontró WeaponManager."
            );
            return;
        }

        // Solo consideramos drops correspondientes a armas elegidas.
        var armasSeleccionadas = weaponManager.ObtenerTiposMunicionSeleccionados();

        if (armasSeleccionadas == null ||
            armasSeleccionadas.Count == 0)
        {
            Debug.LogWarning(
                "EnemyHealth: No hay tipos de munición seleccionados."
            );
            return;
        }

        // Construimos una lista de drops válidos.
        System.Collections.Generic.List<AmmoDrop> dropsValidos =
            new System.Collections.Generic.List<AmmoDrop>();

        float probabilidadTotal = 0f;

        foreach (AmmoDrop drop in dropsMunicion)
        {
            if (drop == null || drop.prefab == null ||
                drop.probabilidad <= 0f)
                continue;

            if (!armasSeleccionadas.Contains(drop.tipoMunicion))
                continue;

            dropsValidos.Add(drop);
            probabilidadTotal += drop.probabilidad;
        }

        if (dropsValidos.Count == 0 || probabilidadTotal <= 0f)
            return;

        // Las probabilidades se conservan como pesos relativos.
        // La suma total también define la probabilidad de no soltar nada.
        float resultado = Random.Range(0f, 100f);

        if (resultado >= Mathf.Min(probabilidadTotal, 100f))
            return;

        float acumulado = 0f;

        foreach (AmmoDrop drop in dropsValidos)
        {
            acumulado += drop.probabilidad;

            if (resultado > acumulado)
                continue;

            Vector3 posicionDrop = transform.position;
            Vector3 origenRaycast =
                transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(
                origenRaycast,
                Vector3.down,
                out RaycastHit hit,
                10f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                posicionDrop = hit.point + Vector3.up * 0.1f;
            }
            else
            {
                posicionDrop += Vector3.down * 0.3f;
            }

            GameObject objetoDrop = Instantiate(
                drop.prefab,
                posicionDrop,
                Quaternion.identity
            );

            AmmoPickup pickup =
                objetoDrop.GetComponent<AmmoPickup>();

            if (pickup == null)
                pickup = objetoDrop.GetComponentInChildren<AmmoPickup>();

            if (pickup != null)
            {
                // Conserva el tipo y la cantidad configurados en el prefab.
                pickup.Configurar(drop.tipoMunicion, pickup.CantidadConfigurada);
            }

            Debug.Log(
                "Drop de munición generado: " + drop.tipoMunicion
            );

            return;
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
}
