
using UnityEngine;
using UnityEngine.UI;

public class IndicadorPuntoSpawn : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform puntoSpawn;
    [SerializeField] private Camera camaraPlanificacion;
    [SerializeField] private RectTransform icono;

    [Header("Posición en pantalla")]
    [SerializeField] private Vector2 margenBorde = new Vector2(60f, 60f);
    [SerializeField] private float desplazamientoVertical = -100f;

    [Header("Orientación")]
    [SerializeField] private bool girarHaciaGenerador = true;
    [SerializeField] private float correccionRotacion = -90f;

    private Image imagenIcono;
    private Canvas canvas;
    private RectTransform rectTransformCanvas;
    private bool mostrarIndicador;

    private void Awake()
    {
        ObtenerReferencias();
    }

    private void ObtenerReferencias()
    {
        if (icono == null)
            return;

        imagenIcono = icono.GetComponent<Image>();
        canvas = icono.GetComponentInParent<Canvas>();

        if (canvas != null)
            rectTransformCanvas = canvas.transform as RectTransform;

        if (imagenIcono != null)
            imagenIcono.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (imagenIcono == null)
            ObtenerReferencias();

        if (imagenIcono == null)
            return;

        imagenIcono.enabled = mostrarIndicador;

        if (!mostrarIndicador ||
            puntoSpawn == null ||
            camaraPlanificacion == null ||
            icono == null ||
            canvas == null ||
            rectTransformCanvas == null)
        {
            return;
        }

        Vector3 posicionViewport =
            camaraPlanificacion.WorldToViewportPoint(
                puntoSpawn.position
            );

        Vector2 direccion = new Vector2(
            posicionViewport.x - 0.5f,
            posicionViewport.y - 0.5f
        );

        if (posicionViewport.z < 0f)
            direccion = -direccion;

        if (direccion.sqrMagnitude < 0.001f)
            direccion = Vector2.up;

        direccion.Normalize();

        float ancho = rectTransformCanvas.rect.width;
        float alto = rectTransformCanvas.rect.height;

        float limiteX = ancho * 0.5f - margenBorde.x;
        float limiteY = alto * 0.5f - margenBorde.y;

        float factorX = Mathf.Abs(direccion.x) > 0.001f
            ? limiteX / Mathf.Abs(direccion.x)
            : float.MaxValue;

        float factorY = Mathf.Abs(direccion.y) > 0.001f
            ? limiteY / Mathf.Abs(direccion.y)
            : float.MaxValue;

        float factor = Mathf.Min(factorX, factorY);

        Vector2 posicionBorde = direccion * factor;
        posicionBorde.y += desplazamientoVertical;

        icono.anchoredPosition = posicionBorde;

        if (girarHaciaGenerador)
        {
            float angulo =
                Mathf.Atan2(direccion.y, direccion.x) *
                Mathf.Rad2Deg;

            icono.localRotation = Quaternion.Euler(
                0f,
                0f,
                angulo + correccionRotacion
            );
        }
    }

    public void MostrarIndicador()
    {
        mostrarIndicador = true;

        if (imagenIcono == null)
            ObtenerReferencias();

        if (imagenIcono != null)
            imagenIcono.enabled = true;
    }

    public void OcultarIndicador()
    {
        mostrarIndicador = false;

        if (imagenIcono == null)
            ObtenerReferencias();

        if (imagenIcono != null)
            imagenIcono.enabled = false;
    }

    public void Configurar(
        Transform nuevoPuntoSpawn,
        Camera nuevaCamara,
        RectTransform nuevoIcono)
    {
        puntoSpawn = nuevoPuntoSpawn;
        camaraPlanificacion = nuevaCamara;
        icono = nuevoIcono;

        ObtenerReferencias();
    }
}
