
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EfectoDanioObjetivo : MonoBehaviour
{
    [Header("Entidad con vida")]
    [SerializeField] private Health entidadVida;

    [Header("Barra de vida")]
    [SerializeField] private Image barraVida;

    [Header("Configuración del parpadeo")]
    [SerializeField] private Color colorDanio = Color.red;
    [SerializeField] private float duracionEfecto = 0.6f;
    [SerializeField] private float velocidadParpadeo = 0.1f;

    private float vidaAnterior;
    private Color colorOriginal;

    private Coroutine corrutinaParpadeo;

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

        if (entidadVida == null || barraVida == null)
        {
            Debug.LogError(
                "Faltan referencias en EfectoDanioObjetivo.",
                this
            );

            enabled = false;
            return;
        }

        vidaAnterior = entidadVida.VidaActual;
        colorOriginal = barraVida.color;
    }

    private void Update()
    {
        float vidaActual = entidadVida.VidaActual;

        if (vidaActual < vidaAnterior)
        {
            IniciarEfecto();
        }

        vidaAnterior = vidaActual;
    }

    private void IniciarEfecto()
    {
        if (corrutinaParpadeo != null)
        {
            StopCoroutine(corrutinaParpadeo);
        }

        corrutinaParpadeo = StartCoroutine(Parpadear());
    }

    private IEnumerator Parpadear()
    {
        float tiempoTranscurrido = 0f;
        bool mostrarRojo = true;

        while (tiempoTranscurrido < duracionEfecto)
        {
            barraVida.color = mostrarRojo
                ? colorDanio
                : colorOriginal;

            mostrarRojo = !mostrarRojo;

            yield return new WaitForSeconds(velocidadParpadeo);

            tiempoTranscurrido += velocidadParpadeo;
        }

        barraVida.color = colorOriginal;
        corrutinaParpadeo = null;
    }

    private void OnDisable()
    {
        if (corrutinaParpadeo != null)
        {
            StopCoroutine(corrutinaParpadeo);
            corrutinaParpadeo = null;
        }

        if (barraVida != null)
        {
            barraVida.color = colorOriginal;
        }
    }
}
