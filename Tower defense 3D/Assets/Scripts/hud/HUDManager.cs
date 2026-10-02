using System.Collections;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Vida")]
    [SerializeField] private TMP_Text textoVida;

    [Header("Horda")]
    [SerializeField] private TMP_Text textoHorda;

    [Header("Enemigos")]
    [SerializeField] private TMP_Text textoEnemigos;

    [Header("Oleadas")]
    [SerializeField] private WaveManager waveManager;

    [Header("Arma")]
    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private TMP_Text textoNombreArma;

    [SerializeField] private TMP_Text textoMunicion;

    [Header("Aviso de nueva horda")]
    [SerializeField] private TMP_Text textoAvisoHorda;

    [SerializeField] private float duracionAvisoHorda = 2f;

    [Header("Animación del aviso")]
    [SerializeField] private float escalaInicial = 1.5f;

    [SerializeField] private float velocidadEntrada = 8f;

    [SerializeField] private float velocidadSalida = 5f;

    private int ultimaHordaMostrada = -1;

    private Coroutine coroutineAviso;

    private RectTransform rectTransformAviso;

    private Color colorOriginalAviso;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth =
                FindFirstObjectByType<PlayerHealth>();
        }

        if (waveManager == null)
        {
            waveManager =
                FindFirstObjectByType<WaveManager>();
        }

        if (weaponManager == null)
        {
            weaponManager =
                FindFirstObjectByType<WeaponManager>();
        }

        // =====================================
        // CONFIGURAR AVISO
        // =====================================

        if (textoAvisoHorda != null)
        {
            rectTransformAviso =
                textoAvisoHorda.GetComponent<RectTransform>();

            colorOriginalAviso =
                textoAvisoHorda.color;

            textoAvisoHorda.text = "";

            textoAvisoHorda.gameObject.SetActive(
                false
            );
        }
    }

    private void Update()
    {
        ActualizarVida();
        ActualizarOleada();
        ActualizarEnemigos();
        ActualizarArma();
        ComprobarNuevaHorda();
    }

    // =========================================
    // VIDA
    // =========================================

    private void ActualizarVida()
    {
        if (playerHealth == null ||
            textoVida == null)
            return;

        float vidaActual =
            playerHealth.VidaActual;

        float vidaMaxima =
            playerHealth.VidaMaxima;

        textoVida.text =
            Mathf.CeilToInt(vidaActual) +
            " / " +
            Mathf.CeilToInt(vidaMaxima);

        if (vidaMaxima <= 0)
            return;

        float porcentaje =
            (vidaActual / vidaMaxima) * 100f;

        if (porcentaje >= 70f)
        {
            textoVida.color =
                Color.green;
        }
        else if (porcentaje >= 31f)
        {
            textoVida.color =
                Color.yellow;
        }
        else
        {
            textoVida.color =
                Color.red;
        }
    }

    // =========================================
    // HORDA
    // =========================================

    private void ActualizarOleada()
    {
        if (waveManager == null ||
            textoHorda == null)
            return;

        if (waveManager.TodasLasOleadasTerminadas)
        {
            textoHorda.text =
                "TODAS LAS HORDAS COMPLETADAS";

            return;
        }

        textoHorda.text =
            "HORDA " +
            waveManager.OleadaActual +
            " / " +
            waveManager.TotalOleadas;
    }

    // =========================================
    // ENEMIGOS
    // =========================================

    private void ActualizarEnemigos()
    {
        if (waveManager == null ||
            textoEnemigos == null)
            return;

        textoEnemigos.text =
            "ENEMIGOS: " +
            waveManager.EnemigosRestantes +
            " / " +
            waveManager.EnemigosTotales;
    }

    // =========================================
    // ARMA
    // =========================================

    private void ActualizarArma()
    {
        if (weaponManager == null)
            return;

        Weapon armaActual =
            weaponManager.GetArmaActual();

        if (armaActual == null)
            return;

        if (textoNombreArma != null)
        {
            textoNombreArma.text =
                armaActual.nombreArma;
        }

        if (textoMunicion != null)
        {
            textoMunicion.text =
                armaActual.MunicionActual +
                " / " +
                armaActual.MunicionReserva;
        }
    }

    // =========================================
    // DETECTAR NUEVA HORDA
    // =========================================

    private void ComprobarNuevaHorda()
    {
        if (waveManager == null)
            return;

        int hordaActual =
            waveManager.OleadaActual;

        if (hordaActual <= 0)
            return;

        if (hordaActual ==
            ultimaHordaMostrada)
            return;

        ultimaHordaMostrada =
            hordaActual;

        MostrarAvisoHorda(
            hordaActual
        );
    }

    // =========================================
    // MOSTRAR AVISO
    // =========================================

    private void MostrarAvisoHorda(
        int numeroHorda)
    {
        if (textoAvisoHorda == null)
            return;

        if (coroutineAviso != null)
        {
            StopCoroutine(
                coroutineAviso
            );
        }

        coroutineAviso =
            StartCoroutine(
                AnimarAvisoHorda(
                    numeroHorda
                )
            );
    }

    // =========================================
    // ANIMACIÓN
    // =========================================

    private IEnumerator AnimarAvisoHorda(
        int numeroHorda)
    {
        textoAvisoHorda.gameObject.SetActive(
            true
        );

        // =====================================
        // TEXTO
        // =====================================

        if (numeroHorda >=
            waveManager.TotalOleadas)
        {
            textoAvisoHorda.text =
                "HORDA " +
                numeroHorda +
                "\n¡ÚLTIMA HORDA!";
        }
        else
        {
            textoAvisoHorda.text =
                "HORDA " +
                numeroHorda +
                "\n¡PREPÁRATE!";
        }

        // =====================================
        // ESTADO INICIAL
        // =====================================

        rectTransformAviso.localScale =
            Vector3.one *
            escalaInicial;

        Color color =
            colorOriginalAviso;

        color.a = 1f;

        textoAvisoHorda.color =
            color;

        // =====================================
        // ENTRADA
        // =====================================

        while (
            Vector3.Distance(
                rectTransformAviso.localScale,
                Vector3.one
            ) > 0.01f)
        {
            rectTransformAviso.localScale =
                Vector3.Lerp(
                    rectTransformAviso.localScale,
                    Vector3.one,
                    velocidadEntrada *
                    Time.deltaTime
                );

            yield return null;
        }

        rectTransformAviso.localScale =
            Vector3.one;

        // =====================================
        // MANTENER VISIBLE
        // =====================================

        float tiempoVisible =
            Mathf.Max(
                0f,
                duracionAvisoHorda
            );

        yield return new WaitForSeconds(
            tiempoVisible
        );

        // =====================================
        // DESAPARECER
        // =====================================

        Color colorFade =
            textoAvisoHorda.color;

        while (colorFade.a > 0.01f)
        {
            colorFade.a =
                Mathf.Lerp(
                    colorFade.a,
                    0f,
                    velocidadSalida *
                    Time.deltaTime
                );

            textoAvisoHorda.color =
                colorFade;

            yield return null;
        }

        colorFade.a = 0f;

        textoAvisoHorda.color =
            colorFade;

        textoAvisoHorda.text = "";

        textoAvisoHorda.gameObject.SetActive(
            false
        );

        // Restaurar el color original
        textoAvisoHorda.color =
            colorOriginalAviso;

        coroutineAviso = null;
    }
}