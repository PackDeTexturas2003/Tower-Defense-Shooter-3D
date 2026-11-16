
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WeaponMuzzleFlash : MonoBehaviour
{
    [System.Serializable]
    public class ConfiguracionArma
    {
        public AmmoType tipoArma;

        [Header("Color")]
        public Color colorDestello = new Color(1f, 0.7f, 0.2f);

        [Header("Cono")]
        [Min(0.01f)] public float longitud = 0.65f;
        [Range(1f, 80f)] public float apertura = 22f;
        [Min(0.01f)] public float grosor = 0.12f;

        [Header("Intensidad")]
        [Min(0f)] public float intensidad = 5f;

        [Header("Duración")]
        [Min(0.01f)] public float duracion = 0.06f;
    }

    [Header("Configuración por arma")]
    [SerializeField]
    private ConfiguracionArma[] configuraciones =
    {
        new ConfiguracionArma
        {
            tipoArma = AmmoType.Pistola,
            colorDestello = new Color(1f, 0.8f, 0.35f),
            longitud = 0.45f,
            apertura = 20f,
            grosor = 0.09f,
            intensidad = 5f,
            duracion = 0.04f
        },
        new ConfiguracionArma
        {
            tipoArma = AmmoType.Shotgun,
            colorDestello = new Color(1f, 0.5f, 0.12f),
            longitud = 0.8f,
            apertura = 35f,
            grosor = 0.18f,
            intensidad = 9f,
            duracion = 0.08f
        },
        new ConfiguracionArma
        {
            tipoArma = AmmoType.SMG,
            colorDestello = new Color(1f, 0.75f, 0.3f),
            longitud = 0.4f,
            apertura = 18f,
            grosor = 0.08f,
            intensidad = 5f,
            duracion = 0.025f
        },
        new ConfiguracionArma
        {
            tipoArma = AmmoType.Rifle,
            colorDestello = new Color(1f, 0.65f, 0.2f),
            longitud = 1.2f,
            apertura = 32f,
            grosor = 0.20f,
            intensidad = 10f,
            duracion = 0.05f
        },
        new ConfiguracionArma
        {
            tipoArma = AmmoType.Sniper,
            colorDestello = new Color(1f, 0.9f, 0.65f),
            longitud = 1.8f,
            apertura = 35f,
            grosor = 0.28f,
            intensidad = 16f,
            duracion = 0.09f
        },
        new ConfiguracionArma
        {
            tipoArma = AmmoType.RPG,
            colorDestello = new Color(1f, 0.3f, 0.05f),
            longitud = 2f,
            apertura = 45f,
            grosor = 0.35f,
            intensidad = 20f,
            duracion = 0.13f
        }
    };

    [Header("Referencias")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform puntoDestello;

    [Header("Iluminación")]
    [SerializeField] private bool usarLuz = true;
    [SerializeField] private float intensidadLuz = 12f;
    [SerializeField] private float alcanceLuz = 1.5f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Material material;

    private Light luz;
    private Weapon armaAnterior;
    private int municionAnterior = -1;
    private Coroutine rutinaDestello;

    private void Awake()
    {
        CrearCono();
        CrearLuz();
        OcultarDestello();
    }

    private void Start()
    {
        if (weaponManager == null)
            weaponManager = FindFirstObjectByType<WeaponManager>();

        if (weaponManager == null)
            Debug.LogError("No se encontró WeaponManager.");
    }

    private void CrearCono()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "Malla_Destello";

        meshFilter.sharedMesh = mesh;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        material = new Material(shader);
        material.renderQueue = 3000;

        material.SetInt("_Cull", 0);

        meshRenderer.material = material;
        meshRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        meshRenderer.receiveShadows = false;
    }

    private void CrearLuz()
    {
        GameObject objeto = new GameObject("Luz_Destello");

        objeto.transform.SetParent(transform);
        objeto.transform.localPosition = Vector3.zero;

        luz = objeto.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.shadows = LightShadows.None;
        luz.intensity = 0f;
        luz.enabled = false;
    }

    private void Update()
    {
        if (weaponManager == null)
            return;

        Weapon armaActual = weaponManager.GetArmaActual();

        if (armaActual == null)
            return;

        if (armaActual != armaAnterior)
        {
            armaAnterior = armaActual;
            municionAnterior = armaActual.MunicionActual;
            return;
        }

        int municionActual = armaActual.MunicionActual;

        if (municionActual < municionAnterior)
            ActivarDestello(armaActual.TipoMunicion);

        municionAnterior = municionActual;
    }

    private void ActivarDestello(AmmoType tipoArma)
    {
        ConfiguracionArma config = ObtenerConfiguracion(tipoArma);

        if (config == null)
            return;

        if (rutinaDestello != null)
            StopCoroutine(rutinaDestello);

        rutinaDestello = StartCoroutine(
            ReproducirDestello(config)
        );
    }

    private ConfiguracionArma ObtenerConfiguracion(AmmoType tipo)
    {
        foreach (ConfiguracionArma config in configuraciones)
        {
            if (config.tipoArma == tipo)
                return config;
        }

        return null;
    }

    private IEnumerator ReproducirDestello(ConfiguracionArma config)
    {
        transform.position = puntoDestello != null
            ? puntoDestello.position
            : transform.position;

        transform.rotation = puntoDestello != null
            ? puntoDestello.rotation
            : transform.rotation;

        transform.localScale = Vector3.one;

        CrearGeometria(config);

        meshRenderer.enabled = true;

        if (usarLuz && luz != null)
        {
            luz.color = config.colorDestello;
            luz.range = alcanceLuz;
            luz.intensity = intensidadLuz;
            luz.enabled = true;
        }

        float tiempo = 0f;

        while (tiempo < config.duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / config.duracion
            );

            // El cono se encoge y desvanece.
            float escala = 1f - progreso;

            transform.localScale = new Vector3(
                escala,
                escala,
                escala
            );

            float alpha = 1f - progreso;

            Color color = config.colorDestello;
            color.a = alpha;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            if (luz != null && usarLuz)
                luz.intensity = intensidadLuz * alpha;

            yield return null;
        }

        OcultarDestello();
        rutinaDestello = null;
    }

    private void CrearGeometria(ConfiguracionArma config)
    {
        mesh.Clear();

        int segmentos = 20;

        Vector3[] vertices = new Vector3[segmentos + 2];
        int[] triangulos = new int[segmentos * 3];

        // La punta del cono comienza en el origen.
        vertices[0] = Vector3.zero;

        float radio = Mathf.Tan(
            config.apertura * Mathf.Deg2Rad
        ) * config.longitud;

        for (int i = 0; i <= segmentos; i++)
        {
            float angulo = i * Mathf.PI * 2f / segmentos;

            vertices[i + 1] = new Vector3(
                Mathf.Cos(angulo) * radio,
                Mathf.Sin(angulo) * radio,
                config.longitud
            );
        }

        for (int i = 0; i < segmentos; i++)
        {
            triangulos[i * 3] = 0;
            triangulos[i * 3 + 1] = i + 1;
            triangulos[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Color color = config.colorDestello;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }

    private void OcultarDestello()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (luz != null)
        {
            luz.intensity = 0f;
            luz.enabled = false;
        }

        if (transform != null)
            transform.localScale = Vector3.one;
    }

    private void OnDisable()
    {
        if (rutinaDestello != null)
        {
            StopCoroutine(rutinaDestello);
            rutinaDestello = null;
        }

        OcultarDestello();
    }

    private void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);

        if (material != null)
            Destroy(material);
    }
}
