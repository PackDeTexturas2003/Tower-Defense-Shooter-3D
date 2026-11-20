
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

    [Header("Sonido al morir")]
    [SerializeField] private AudioClip sonidoMuerte;
    [SerializeField, Range(0f, 5f)] private float volumenSonidoMuerte = 2f;
    [SerializeField, Min(0.1f)] private float distanciaMinimaAudio = 2f;
    [SerializeField, Min(1f)] private float distanciaMaximaAudio = 25f;

    [Header("Drops de Munición")]
    [SerializeField] private AmmoDrop[] dropsMunicion;

    [Header("Recompensa de dinero")]
    [SerializeField] private int dineroAlMorir = 50;

    private Color[] coloresOriginales;
    private Coroutine efectoDanio;
    private bool yaMurio;
    private int ultimoFrameSonido = -1;

    // Contador compartido para espaciar sonidos de muertes ocurridas
    // en el mismo frame y evitar que queden completamente superpuestos.
    private static int ultimoFrameAudioMuerte = -1;
    private static int sonidosMuerteEnEsteFrame = 0;

    [Header("Separación entre sonidos simultáneos")]
    [SerializeField, Min(0f)] private float retrasoEntreSonidos = 0.12f;

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

        // Reproduce el sonido en la posición del enemigo.
        // PlayClipAtPoint crea un AudioSource temporal que continúa
        // reproduciéndose aunque este enemigo sea destruido.
        if (sonidoMuerte != null)
        {
            ReproducirSonidoMuerte();
        }

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


    private void ReproducirSonidoMuerte()
    {
        // Cada muerte crea su propio AudioSource temporal. Así, si mueren
        // dos o más enemigos juntos, cada uno reproduce su propio sonido.
        // El objeto temporal permite que el audio termine aunque el enemigo
        // sea destruido al finalizar su animación.
        GameObject objetoAudio = new GameObject("AudioMuerteEnemigo");
        objetoAudio.transform.position = transform.position;

        AudioSource audioSource = objetoAudio.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Audio 3D: depende de la distancia al jugador.
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = Mathf.Max(0.1f, distanciaMinimaAudio);
        audioSource.maxDistance = Mathf.Max(
            audioSource.minDistance + 0.1f,
            distanciaMaximaAudio
        );
        audioSource.dopplerLevel = 0f;
        audioSource.volume = 1f;

        // Cada enemigo crea su propio AudioSource y su propia instancia
        // del clip. Si varias muertes ocurren en el mismo frame, se
        // reproducen con una pequeña separación para que se distingan.
        audioSource.clip = CrearClipConGanancia(sonidoMuerte, volumenSonidoMuerte);

        if (ultimoFrameAudioMuerte != Time.frameCount)
        {
            ultimoFrameAudioMuerte = Time.frameCount;
            sonidosMuerteEnEsteFrame = 0;
        }

        float retraso = sonidosMuerteEnEsteFrame * retrasoEntreSonidos;
        sonidosMuerteEnEsteFrame++;

        audioSource.PlayDelayed(retraso);

        Destroy(objetoAudio, retraso + audioSource.clip.length + 0.2f);
    }

    private AudioClip CrearClipConGanancia(AudioClip original, float ganancia)
    {
        if (original == null || Mathf.Approximately(ganancia, 1f))
            return original;

        int totalMuestras = original.samples * original.channels;
        float[] muestras = new float[totalMuestras];

        if (!original.GetData(muestras, 0))
        {
            Debug.LogWarning("EnemyHealth: No se pudo amplificar el audio. Revisa que el AudioClip permita leer datos.");
            return original;
        }

        for (int i = 0; i < muestras.Length; i++)
            muestras[i] = Mathf.Clamp(muestras[i] * ganancia, -1f, 1f);

        AudioClip amplificado = AudioClip.Create(
            original.name + "_Amplificado",
            original.samples,
            original.channels,
            original.frequency,
            false
        );

        amplificado.SetData(muestras, 0);
        return amplificado;
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
