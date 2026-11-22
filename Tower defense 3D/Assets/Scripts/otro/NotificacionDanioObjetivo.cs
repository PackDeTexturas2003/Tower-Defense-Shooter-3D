
using UnityEngine;
using TMPro;
using System.Collections;

public class NotificacionDanioObjetivo : MonoBehaviour
{
    [Header("Entidad con vida")]
    [SerializeField] private Health entidadVida;

    [Header("Notificación")]
    [SerializeField] private TMP_Text textoNotificacion;
    [SerializeField] private string mensaje = "¡Están atacando el objetivo!";

    [Header("Configuración")]
    [SerializeField] private float duracionNotificacion = 2f;

    [Header("Animación de aparición")]
    [SerializeField] private float duracionCrecimiento = 0.2f;
    [SerializeField] private float escalaRebote = 1.15f;
    [SerializeField] private float duracionRebote = 0.12f;

    private float vidaAnterior;

    private bool notificacionMostrada;

    private Coroutine corrutinaNotificacion;

    private RectTransform rectTransformTexto;
    private Vector3 escalaOriginal;

    private void Awake()
    {
        if (entidadVida == null)
        {
            entidadVida = GetComponentInParent<Health>();

            if (entidadVida == null)
            {
                entidadVida = GetComponentInChildren<Health>();
            }
        }

        if (entidadVida == null || textoNotificacion == null)
        {
            Debug.LogError(
                "Faltan referencias en NotificacionDanioObjetivo.",
                this
            );

            enabled = false;
            return;
        }

        vidaAnterior = entidadVida.VidaActual;

        rectTransformTexto = textoNotificacion.GetComponent<RectTransform>();
        escalaOriginal = rectTransformTexto.localScale;

        textoNotificacion.text = mensaje;
        textoNotificacion.gameObject.SetActive(false);
    }

    private void Update()
    {
        float vidaActual = entidadVida.VidaActual;

        if (vidaActual < vidaAnterior && !notificacionMostrada)
        {
            MostrarNotificacion();
        }

        vidaAnterior = vidaActual;
    }

    private void MostrarNotificacion()
    {
        notificacionMostrada = true;

        corrutinaNotificacion = StartCoroutine(
            AnimarNotificacion()
        );
    }

    private IEnumerator AnimarNotificacion()
    {
        textoNotificacion.text = mensaje;
        textoNotificacion.gameObject.SetActive(true);

        rectTransformTexto.localScale = Vector3.zero;

        Vector3 escalaReboteFinal = escalaOriginal * escalaRebote;

        float tiempo = 0f;

        // Crecimiento inicial.
        while (tiempo < duracionCrecimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionCrecimiento
            );

            rectTransformTexto.localScale = Vector3.Lerp(
                Vector3.zero,
                escalaOriginal,
                progreso
            );

            yield return null;
        }

        // Primer rebote: agrandar.
        yield return StartCoroutine(
            AnimarEscala(
                escalaOriginal,
                escalaReboteFinal,
                duracionRebote
            )
        );

        // Primer rebote: achicar.
        yield return StartCoroutine(
            AnimarEscala(
                escalaReboteFinal,
                escalaOriginal,
                duracionRebote
            )
        );

        // Segundo rebote: agrandar.
        yield return StartCoroutine(
            AnimarEscala(
                escalaOriginal,
                escalaReboteFinal,
                duracionRebote
            )
        );

        // Segundo rebote: finalizar en el tamaño original.
        yield return StartCoroutine(
            AnimarEscala(
                escalaReboteFinal,
                escalaOriginal,
                duracionRebote
            )
        );

        rectTransformTexto.localScale = escalaOriginal;

        // Mantener visible el texto durante el tiempo restante.
        float tiempoVisible = duracionNotificacion -
            duracionCrecimiento -
            (duracionRebote * 4f);

        if (tiempoVisible > 0f)
        {
            yield return new WaitForSeconds(tiempoVisible);
        }

        textoNotificacion.gameObject.SetActive(false);

        corrutinaNotificacion = null;
    }

    private IEnumerator AnimarEscala(
        Vector3 escalaInicial,
        Vector3 escalaFinal,
        float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracion
            );

            rectTransformTexto.localScale = Vector3.Lerp(
                escalaInicial,
                escalaFinal,
                progreso
            );

            yield return null;
        }

        rectTransformTexto.localScale = escalaFinal;
    }

    private void OnDisable()
    {
        if (corrutinaNotificacion != null)
        {
            StopCoroutine(corrutinaNotificacion);
            corrutinaNotificacion = null;
        }

        if (textoNotificacion != null)
        {
            textoNotificacion.gameObject.SetActive(false);
            rectTransformTexto.localScale = escalaOriginal;
        }
    }
}
