using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestorCinematicaUI : MonoBehaviour
{
    public enum TipoAnimacion
    {
        Movimiento,
        AparicionYDesaparicion
    }

    [System.Serializable]
    public class ElementoCinematica
    {
        [Header("Configuracion general")]
        public string nombre;
        public Image imagen;
        public TipoAnimacion tipo;
        [Min(0f)] public float tiempoInicio;

        [Header("Movimiento")]
        public Vector3 posicionInicial;
        public Vector3 posicionFinal;
        [Min(0f)] public float duracionMovimiento = 3f;

        [Header("Aparicion y desaparicion")]
        [Min(0f)] public float duracionAparicion = 1f;
        [Min(0f)] public float tiempoVisible = 2f;
        [Min(0f)] public float duracionDesaparicion = 1f;
    }

    [Header("Elementos de la cinematica")]
    [SerializeField] private ElementoCinematica[] elementos;

    private void Start()
    {
        foreach (ElementoCinematica elemento in elementos)
        {
            if (elemento.imagen == null)
                continue;

            EstablecerOpacidad(elemento.imagen, 0f);
            elemento.imagen.enabled = true;
            elemento.imagen.rectTransform.localPosition =
                elemento.posicionInicial;

            StartCoroutine(EjecutarElemento(elemento));
        }
    }

    private IEnumerator EjecutarElemento(ElementoCinematica elemento)
    {
        yield return new WaitForSecondsRealtime(elemento.tiempoInicio);

        if (elemento.imagen == null)
            yield break;

        switch (elemento.tipo)
        {
            case TipoAnimacion.Movimiento:
                yield return StartCoroutine(MoverImagen(elemento));
                break;

            case TipoAnimacion.AparicionYDesaparicion:
                yield return StartCoroutine(DesvanecerImagen(elemento));
                break;
        }
    }

    private IEnumerator MoverImagen(ElementoCinematica elemento)
    {
        RectTransform rectTransform = elemento.imagen.rectTransform;

        rectTransform.localPosition = elemento.posicionInicial;
        EstablecerOpacidad(elemento.imagen, 1f);

        if (elemento.duracionMovimiento <= 0f)
        {
            rectTransform.localPosition = elemento.posicionFinal;
            yield break;
        }

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < elemento.duracionMovimiento)
        {
            if (elemento.imagen == null)
                yield break;

            tiempoTranscurrido += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(
                tiempoTranscurrido / elemento.duracionMovimiento
            );

            rectTransform.localPosition = Vector3.Lerp(
                elemento.posicionInicial,
                elemento.posicionFinal,
                progreso
            );

            yield return null;
        }

        if (elemento.imagen != null)
            rectTransform.localPosition = elemento.posicionFinal;
    }

    private IEnumerator DesvanecerImagen(ElementoCinematica elemento)
    {
        Image imagen = elemento.imagen;
        EstablecerOpacidad(imagen, 0f);

        yield return StartCoroutine(
            CambiarOpacidad(imagen, 0f, 1f, elemento.duracionAparicion)
        );

        if (imagen == null)
            yield break;

        if (elemento.tiempoVisible > 0f)
            yield return new WaitForSecondsRealtime(elemento.tiempoVisible);

        if (imagen == null)
            yield break;

        yield return StartCoroutine(
            CambiarOpacidad(imagen, 1f, 0f, elemento.duracionDesaparicion)
        );
    }

    private IEnumerator CambiarOpacidad(
        Image imagen,
        float opacidadInicial,
        float opacidadFinal,
        float duracion)
    {
        if (imagen == null)
            yield break;

        if (duracion <= 0f)
        {
            EstablecerOpacidad(imagen, opacidadFinal);
            yield break;
        }

        float tiempoTranscurrido = 0f;

        EstablecerOpacidad(imagen, opacidadInicial);

        while (tiempoTranscurrido < duracion)
        {
            if (imagen == null)
                yield break;

            tiempoTranscurrido += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(tiempoTranscurrido / duracion);

            float opacidad = Mathf.Lerp(
                opacidadInicial,
                opacidadFinal,
                progreso
            );

            EstablecerOpacidad(imagen, opacidad);

            yield return null;
        }

        if (imagen != null)
            EstablecerOpacidad(imagen, opacidadFinal);
    }

    private void EstablecerOpacidad(Image imagen, float opacidad)
    {
        if (imagen == null)
            return;

        Color color = imagen.color;
        color.a = opacidad;
        imagen.color = color;
    }
}