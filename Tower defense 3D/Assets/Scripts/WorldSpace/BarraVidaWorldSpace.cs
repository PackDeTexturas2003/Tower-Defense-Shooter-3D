
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraVidaWorldSpace : MonoBehaviour
{
    [Header("Entidad con vida")]
    [SerializeField] private Health entidadVida;

    [Header("Elementos de la interfaz")]
    [SerializeField] private Image barraVida;
    [SerializeField] private TMP_Text textoVida;

    [Header("Jugador")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private Transform camaraJugador;

    [Header("Configuración")]
    [SerializeField] private bool mostrarVidaNumerica = true;

    private RectTransform rectTransformCanvas;

    private void Awake()
    {
        rectTransformCanvas = GetComponent<RectTransform>();

        BuscarEntidadVida();
        BuscarCamaraJugador();

        if (entidadVida == null)
        {
            Debug.LogError(
                $"No se encontró un componente Health en los padres ni en los hijos de {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        if (barraVida == null)
        {
            Debug.LogError(
                $"No se asignó la Image de la barra de vida en {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        barraVida.type = Image.Type.Filled;
        barraVida.fillMethod = Image.FillMethod.Horizontal;
        barraVida.fillOrigin = (int)Image.OriginHorizontal.Left;

        ActualizarInterfaz();
    }

    private void LateUpdate()
    {
        if (entidadVida != null)
        {
            ActualizarInterfaz();
        }

        if (camaraJugador == null)
        {
            BuscarCamaraJugador();
        }

        OrientarCanvas();
    }

    private void BuscarEntidadVida()
    {
        if (entidadVida != null)
            return;

        entidadVida = GetComponentInParent<Health>();

        if (entidadVida == null)
        {
            entidadVida = GetComponentInChildren<Health>();
        }
    }

    private void BuscarCamaraJugador()
    {
        if (camaraJugador != null)
            return;

        GameObject jugador = GameObject.FindGameObjectWithTag(tagJugador);

        if (jugador == null)
        {
            Debug.LogWarning(
                $"No se encontró ningún objeto con el tag '{tagJugador}'.",
                this
            );

            return;
        }

        Camera camara = jugador.GetComponentInChildren<Camera>();

        if (camara != null)
        {
            camaraJugador = camara.transform;
        }
        else
        {
            camaraJugador = jugador.transform;
        }
    }

    private void OrientarCanvas()
    {
        if (camaraJugador == null || rectTransformCanvas == null)
            return;

        rectTransformCanvas.rotation = camaraJugador.rotation;
    }

    private void ActualizarInterfaz()
    {
        float vidaMaxima = entidadVida.VidaMaxima;
        float vidaActual = entidadVida.VidaActual;

        barraVida.fillAmount = vidaMaxima > 0f
            ? Mathf.Clamp01(vidaActual / vidaMaxima)
            : 0f;

        if (textoVida != null)
        {
            textoVida.gameObject.SetActive(mostrarVidaNumerica);

            if (mostrarVidaNumerica)
            {
                textoVida.text =
                    $"{Mathf.CeilToInt(vidaActual)} / {Mathf.CeilToInt(vidaMaxima)}";
            }
        }
    }
}
