
using System.Collections;
using UnityEngine;
using TMPro;

public class GestorDialogosCinematica : MonoBehaviour
{
    [System.Serializable]
    public class Dialogo
    {
        [Header("Configuracion general")]
        public string nombre;
        public TextMeshProUGUI texto;

        [TextArea(2, 5)]
        public string contenido;

        [Header("Posicion del dialogo")]
        public Vector3 posicion;

        [Header("Tiempo de inicio")]
        [Min(0f)]
        public float tiempoInicio;

        [Header("Animacion del texto")]
        [Min(0f)]
        public float duracionAparicion = 1f;

        [Min(0f)]
        public float tiempoVisible = 3f;

        [Min(0f)]
        public float duracionDesaparicion = 1f;
    }

    [Header("Dialogos de la cinematica")]
    [SerializeField] private Dialogo[] dialogos;

    private void Start()
    {
        foreach (Dialogo dialogo in dialogos)
        {
            if (dialogo.texto == null)
                continue;

            dialogo.texto.text = "";
            EstablecerOpacidad(dialogo.texto, 0f);

            dialogo.texto.rectTransform.localPosition =
                dialogo.posicion;

            StartCoroutine(EjecutarDialogo(dialogo));
        }
    }

    private IEnumerator EjecutarDialogo(Dialogo dialogo)
    {
        yield return new WaitForSecondsRealtime(dialogo.tiempoInicio);

        if (dialogo.texto == null)
            yield break;

        dialogo.texto.rectTransform.localPosition =
            dialogo.posicion;

        dialogo.texto.text = dialogo.contenido;

        yield return StartCoroutine(
            CambiarOpacidad(
                dialogo.texto,
                0f,
                1f,
                dialogo.duracionAparicion
            )
        );

        if (dialogo.texto == null)
            yield break;

        if (dialogo.tiempoVisible > 0f)
        {
            yield return new WaitForSecondsRealtime(
                dialogo.tiempoVisible
            );
        }

        if (dialogo.texto == null)
            yield break;

        yield return StartCoroutine(
            CambiarOpacidad(
                dialogo.texto,
                1f,
                0f,
                dialogo.duracionDesaparicion
            )
        );

        if (dialogo.texto != null)
            dialogo.texto.text = "";
    }

    private IEnumerator CambiarOpacidad(
        TextMeshProUGUI texto,
        float opacidadInicial,
        float opacidadFinal,
        float duracion)
    {
        if (texto == null)
            yield break;

        if (duracion <= 0f)
        {
            EstablecerOpacidad(texto, opacidadFinal);
            yield break;
        }

        float tiempoTranscurrido = 0f;

        EstablecerOpacidad(texto, opacidadInicial);

        while (tiempoTranscurrido < duracion)
        {
            if (texto == null)
                yield break;

            tiempoTranscurrido += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(
                tiempoTranscurrido / duracion
            );

            float opacidad = Mathf.Lerp(
                opacidadInicial,
                opacidadFinal,
                progreso
            );

            EstablecerOpacidad(texto, opacidad);

            yield return null;
        }

        if (texto != null)
            EstablecerOpacidad(texto, opacidadFinal);
    }

    private void EstablecerOpacidad(
        TextMeshProUGUI texto,
        float opacidad)
    {
        if (texto == null)
            return;

        Color color = texto.color;
        color.a = opacidad;
        texto.color = color;
    }
}
