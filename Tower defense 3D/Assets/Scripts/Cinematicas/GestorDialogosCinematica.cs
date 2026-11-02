using System.Collections.Generic;
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

    [Header("Reloj compartido")]
    [SerializeField]
    private TransicionFinalCinematica reloj;

    [Header("Dialogos de la cinematica")]
    [SerializeField]
    private Dialogo[] dialogos;

    private readonly Dictionary<TextMeshProUGUI, float> opacidadesOriginales =
        new Dictionary<TextMeshProUGUI, float>();

    private void Awake()
    {
        if (reloj == null)
        {
            reloj = FindFirstObjectByType<TransicionFinalCinematica>();
        }
    }

    private void Start()
    {
        if (dialogos == null)
            return;

        foreach (Dialogo dialogo in dialogos)
        {
            if (dialogo.texto == null)
                continue;

            if (!opacidadesOriginales.ContainsKey(dialogo.texto))
            {
                opacidadesOriginales[dialogo.texto] =
                    dialogo.texto.color.a;
            }

            dialogo.texto.rectTransform.localPosition =
                dialogo.posicion;

            dialogo.texto.text = "";

            EstablecerOpacidad(dialogo.texto, 0f);
        }

        ActualizarDialogos();
    }

    private void Update()
    {
        ActualizarDialogos();
    }

    private void ActualizarDialogos()
    {
        if (reloj == null || dialogos == null)
            return;

        float tiempoActual = reloj.TiempoActual;

        foreach (Dialogo dialogo in dialogos)
        {
            if (dialogo.texto == null)
                continue;

            dialogo.texto.rectTransform.localPosition =
                dialogo.posicion;

            float tiempoDialogo =
                tiempoActual - dialogo.tiempoInicio;

            if (tiempoDialogo < 0f)
            {
                OcultarDialogo(dialogo);
                continue;
            }

            float aparicion = Mathf.Max(0f, dialogo.duracionAparicion);
            float visible = Mathf.Max(0f, dialogo.tiempoVisible);
            float desaparicion = Mathf.Max(0f, dialogo.duracionDesaparicion);

            float duracionTotal = aparicion + visible + desaparicion;

            if (tiempoDialogo >= duracionTotal)
            {
                OcultarDialogo(dialogo);
                continue;
            }

            dialogo.texto.text = dialogo.contenido;

            float opacidad;

            if (aparicion > 0f && tiempoDialogo < aparicion)
            {
                float progreso = Mathf.Clamp01(
                    tiempoDialogo / aparicion
                );

                opacidad = Mathf.Lerp(0f, 1f, progreso);
            }
            else
            {
                float tiempoRestante = tiempoDialogo - aparicion;

                if (tiempoRestante < visible)
                {
                    opacidad = 1f;
                }
                else if (desaparicion > 0f)
                {
                    float tiempoDesaparicion =
                        tiempoRestante - visible;

                    float progreso = Mathf.Clamp01(
                        tiempoDesaparicion / desaparicion
                    );

                    opacidad = Mathf.Lerp(1f, 0f, progreso);
                }
                else
                {
                    opacidad = 0f;
                }
            }

            EstablecerOpacidad(dialogo.texto, opacidad);
        }
    }

    private void OcultarDialogo(Dialogo dialogo)
    {
        if (dialogo.texto == null)
            return;

        dialogo.texto.text = "";
        EstablecerOpacidad(dialogo.texto, 0f);
    }

    private void EstablecerOpacidad(
        TextMeshProUGUI texto,
        float opacidad)
    {
        if (texto == null)
            return;

        if (!opacidadesOriginales.TryGetValue(texto, out float alphaOriginal))
        {
            alphaOriginal = texto.color.a;
            opacidadesOriginales[texto] = alphaOriginal;
        }

        Color color = texto.color;
        color.a = alphaOriginal * Mathf.Clamp01(opacidad);
        texto.color = color;
    }
}