
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

        [Min(0f)]
        public float tiempoInicio;

        [Header("Movimiento")]
        public Vector3 posicionInicial;
        public Vector3 posicionFinal;

        [Min(0f)]
        public float duracionMovimiento = 3f;

        [Header("Aparicion y desaparicion")]
        [Min(0f)]
        public float duracionAparicion = 1f;

        [Min(0f)]
        public float tiempoVisible = 2f;

        [Min(0f)]
        public float duracionDesaparicion = 1f;
    }

    [Header("Reloj compartido")]
    [SerializeField]
    private TransicionFinalCinematica reloj;

    [Header("Elementos de la cinematica")]
    [SerializeField]
    private ElementoCinematica[] elementos;

    private void Awake()
    {
        if (reloj == null)
        {
            reloj = FindFirstObjectByType<TransicionFinalCinematica>();
        }
    }

    private void Start()
    {
        foreach (ElementoCinematica elemento in elementos)
        {
            if (elemento.imagen == null)
                continue;

            elemento.imagen.enabled = true;
            EstablecerOpacidad(elemento.imagen, 0f);

            elemento.imagen.rectTransform.localPosition =
                elemento.posicionInicial;
        }

        ActualizarElementos();
    }

    private void Update()
    {
        ActualizarElementos();
    }

    private void ActualizarElementos()
    {
        if (reloj == null)
            return;

        float tiempoActual = reloj.TiempoActual;

        foreach (ElementoCinematica elemento in elementos)
        {
            if (elemento.imagen == null)
                continue;

            float tiempoElemento =
                tiempoActual - elemento.tiempoInicio;

            if (tiempoElemento < 0f)
            {
                elemento.imagen.rectTransform.localPosition =
                    elemento.posicionInicial;

                EstablecerOpacidad(elemento.imagen, 0f);
                continue;
            }

            switch (elemento.tipo)
            {
                case TipoAnimacion.Movimiento:
                    ActualizarMovimiento(elemento, tiempoElemento);
                    break;

                case TipoAnimacion.AparicionYDesaparicion:
                    ActualizarDesvanecimiento(elemento, tiempoElemento);
                    break;
            }
        }
    }

    private void ActualizarMovimiento(
        ElementoCinematica elemento,
        float tiempoElemento)
    {
        RectTransform rectTransform =
            elemento.imagen.rectTransform;

        if (elemento.duracionMovimiento <= 0f)
        {
            rectTransform.localPosition = elemento.posicionFinal;
        }
        else
        {
            float progreso = Mathf.Clamp01(
                tiempoElemento / elemento.duracionMovimiento
            );

            rectTransform.localPosition = Vector3.Lerp(
                elemento.posicionInicial,
                elemento.posicionFinal,
                progreso
            );
        }

        EstablecerOpacidad(elemento.imagen, 1f);
    }

    private void ActualizarDesvanecimiento(
        ElementoCinematica elemento,
        float tiempoElemento)
    {
        float aparicion = elemento.duracionAparicion;
        float visible = elemento.tiempoVisible;
        float desaparicion = elemento.duracionDesaparicion;

        float duracionTotal = aparicion + visible + desaparicion;

        if (tiempoElemento >= duracionTotal)
        {
            EstablecerOpacidad(elemento.imagen, 0f);
            return;
        }

        if (aparicion > 0f && tiempoElemento < aparicion)
        {
            float progreso = Mathf.Clamp01(tiempoElemento / aparicion);

            EstablecerOpacidad(
                elemento.imagen,
                Mathf.Lerp(0f, 1f, progreso)
            );

            return;
        }

        float tiempoRestante = tiempoElemento - aparicion;

        if (tiempoRestante < visible)
        {
            EstablecerOpacidad(elemento.imagen, 1f);
            return;
        }

        float tiempoDesaparicion = tiempoRestante - visible;

        if (desaparicion > 0f)
        {
            float progreso = Mathf.Clamp01(
                tiempoDesaparicion / desaparicion
            );

            EstablecerOpacidad(
                elemento.imagen,
                Mathf.Lerp(1f, 0f, progreso)
            );
        }
        else
        {
            EstablecerOpacidad(elemento.imagen, 0f);
        }
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
