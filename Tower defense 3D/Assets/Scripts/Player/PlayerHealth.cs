using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Health
{
    [Header("Información de Vida")]
    [SerializeField] private float mostrarVidaMaxima;
    [SerializeField] private float mostrarVidaActual;

    [Header("Game Over")]
    [SerializeField] private GameOverManager gameOverManager;

    [Header("Efecto de estática al recibir daño")]
    [SerializeField] private RawImage imagenEstatica;
    [SerializeField, Range(0f, 1f)] private float intensidadEstatica = 0.8f;
    [SerializeField, Min(0.05f)] private float duracionEstatica = 0.35f;
    [SerializeField, Min(0.01f)] private float intervaloRuido = 0.04f;
    [SerializeField, Range(16, 256)] private int resolucionRuido = 128;
    [Header("Colores de estática según el daño")]
    [SerializeField] private Color colorDanioNormal = Color.red;
    [SerializeField] private Color colorDanioCebollita = new Color(0.65f, 1f, 0.65f, 1f);

    private bool recibiendoDanioCebollita;
    private Color colorEstaticaActual = Color.red;
    private bool yaMurio;
    private Coroutine rutinaEstatica;
    private Texture2D texturaEstatica;
    private Color32[] pixelesEstatica;

    protected override void Awake()
    {
        base.Awake();

        BuscarGameOverManager();
        PrepararEstatica();
        ActualizarValores();
    }

    private void BuscarGameOverManager()
    {
        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();

        if (gameOverManager == null)
            Debug.LogError("PlayerHealth: No se encontró GameOverManager en la escena.", this);
    }

    private void Update()
    {
        ActualizarValores();
    }

    private void PrepararEstatica()
    {
        if (imagenEstatica == null)
        {
            Debug.LogWarning("PlayerHealth: Asigna la RawImage de estática en el Inspector.", this);
            return;
        }

        texturaEstatica = new Texture2D(
            resolucionRuido,
            resolucionRuido,
            TextureFormat.RGBA32,
            false
        );
        texturaEstatica.name = "TexturaEstaticaJugador";
        texturaEstatica.filterMode = FilterMode.Point;
        texturaEstatica.wrapMode = TextureWrapMode.Repeat;

        pixelesEstatica = new Color32[resolucionRuido * resolucionRuido];
        imagenEstatica.texture = texturaEstatica;
        imagenEstatica.raycastTarget = false;

        Color color = imagenEstatica.color;
        color.a = 0f;
        imagenEstatica.color = color;
        imagenEstatica.gameObject.SetActive(false);
    }

    public override void RecibirDanio(float cantidad)
    {
        if (yaMurio || cantidad <= 0f)
            return;

        float vidaAntes = vidaActual;
        colorEstaticaActual = recibiendoDanioCebollita
            ? colorDanioCebollita
            : colorDanioNormal;

        base.RecibirDanio(cantidad);

        if (vidaActual < vidaAntes && !yaMurio)
            ActivarEstatica();

        ActualizarValores();
    }

    // Este método se utiliza exclusivamente para el daño de CebollitaDamage.
    public void RecibirDanioCebollita(float cantidad)
    {
        if (yaMurio || cantidad <= 0f)
            return;

        recibiendoDanioCebollita = true;
        RecibirDanio(cantidad);
        recibiendoDanioCebollita = false;
    }

    private void ActivarEstatica()
    {
        if (imagenEstatica == null || texturaEstatica == null || yaMurio)
            return;

        if (rutinaEstatica != null)
            StopCoroutine(rutinaEstatica);

        imagenEstatica.gameObject.SetActive(true);
        rutinaEstatica = StartCoroutine(EfectoEstatica());
    }

    private IEnumerator EfectoEstatica()
    {
        float tiempo = 0f;
        float tiempoRuido = intervaloRuido;
        Color color = colorEstaticaActual;
        color.a = intensidadEstatica;
        imagenEstatica.color = color;

        while (tiempo < duracionEstatica)
        {
            tiempo += Time.unscaledDeltaTime;
            tiempoRuido += Time.unscaledDeltaTime;

            if (tiempoRuido >= intervaloRuido)
            {
                GenerarRuido();
                tiempoRuido = 0f;
            }

            float progreso = Mathf.Clamp01(tiempo / duracionEstatica);
            color.a = intensidadEstatica * (1f - progreso);
            imagenEstatica.color = color;
            yield return null;
        }

        color.a = 0f;
        imagenEstatica.color = color;
        imagenEstatica.gameObject.SetActive(false);
        rutinaEstatica = null;
    }

    private void GenerarRuido()
    {
        if (texturaEstatica == null || pixelesEstatica == null)
            return;

        for (int i = 0; i < pixelesEstatica.Length; i++)
        {
            byte gris = (byte)Random.Range(0, 256);
            pixelesEstatica[i] = new Color32(gris, gris, gris, 255);
        }

        texturaEstatica.SetPixels32(pixelesEstatica);
        texturaEstatica.Apply(false, false);
    }

    private void ActualizarValores()
    {
        mostrarVidaMaxima = vidaMaxima;
        mostrarVidaActual = vidaActual;
    }

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;
        Debug.Log("PlayerHealth: El jugador ha muerto. Activando panel de derrota.", this);

        // Detener y ocultar la estática al morir para que no interfiera con el panel.
        if (rutinaEstatica != null)
        {
            StopCoroutine(rutinaEstatica);
            rutinaEstatica = null;
        }

        if (imagenEstatica != null)
        {
            Color color = imagenEstatica.color;
            color.a = 0f;
            imagenEstatica.color = color;
            imagenEstatica.gameObject.SetActive(false);
        }

        // Buscar nuevamente por si la referencia no estaba asignada al iniciar.
        BuscarGameOverManager();

        if (gameOverManager != null)
        {
            gameOverManager.MostrarDerrota(GameOverManager.CausaDerrota.JugadorEliminado);
        }
        else
        {
            Debug.LogError("PlayerHealth: No se puede mostrar la derrota porque falta GameOverManager.", this);
        }
    }

    public void Curar(float cantidad)
    {
        if (yaMurio || cantidad <= 0f)
            return;

        vidaActual += cantidad;
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        ActualizarValores();
    }

    private void OnDestroy()
    {
        if (texturaEstatica != null)
            Destroy(texturaEstatica);
    }
}
