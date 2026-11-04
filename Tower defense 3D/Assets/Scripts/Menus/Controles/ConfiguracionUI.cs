using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ConfiguracionUI : MonoBehaviour
{
    [Header("Textos de controles")]
    [SerializeField] private TMP_Text textoDisparo;
    [SerializeField] private TMP_Text textoEsquive;
    [SerializeField] private TMP_Text textoSalto;
    [SerializeField] private TMP_Text textoSprint;

    [Header("Textos de movimiento")]
    [SerializeField] private TMP_Text textoAdelante;
    [SerializeField] private TMP_Text textoIzquierda;
    [SerializeField] private TMP_Text textoAtras;
    [SerializeField] private TMP_Text textoDerecha;

    [Header("Botones")]
    [SerializeField] private Button botonDisparo;
    [SerializeField] private Button botonEsquive;
    [SerializeField] private Button botonSalto;
    [SerializeField] private Button botonSprint;

    [Header("Botones de movimiento")]
    [SerializeField] private Button botonAdelante;
    [SerializeField] private Button botonIzquierda;
    [SerializeField] private Button botonAtras;
    [SerializeField] private Button botonDerecha;

    [Header("Restaurar configuraciones")]
    [SerializeField] private Button botonRestaurarPredeterminados;

    private enum ControlEsperando
    {
        Ninguno,
        Disparo,
        Esquive,
        Salto,
        Sprint,
        Adelante,
        Izquierda,
        Atras,
        Derecha
    }

    private ControlEsperando controlEsperando =
        ControlEsperando.Ninguno;

    private void Start()
    {
        ActualizarTextos();

        botonDisparo.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Disparo)
        );

        botonEsquive.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Esquive)
        );

        botonSalto.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Salto)
        );

        botonSprint.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Sprint)
        );

        botonAdelante.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Adelante)
        );

        botonIzquierda.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Izquierda)
        );

        botonAtras.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Atras)
        );

        botonDerecha.onClick.AddListener(
            () => EsperarTecla(ControlEsperando.Derecha)
        );

        botonRestaurarPredeterminados.onClick.AddListener(
            RestaurarPredeterminados
        );
    }

    private void Update()
    {
        if (controlEsperando == ControlEsperando.Ninguno)
            return;

        DetectarEntrada();
    }

    // =========================
    // ESPERAR ENTRADA
    // =========================

    private void EsperarTecla(ControlEsperando control)
    {
        controlEsperando = control;

        switch (control)
        {
            case ControlEsperando.Disparo:
                textoDisparo.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Esquive:
                textoEsquive.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Salto:
                textoSalto.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Sprint:
                textoSprint.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Adelante:
                textoAdelante.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Izquierda:
                textoIzquierda.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Atras:
                textoAtras.text = "Pulsa una tecla...";
                break;

            case ControlEsperando.Derecha:
                textoDerecha.text = "Pulsa una tecla...";
                break;
        }
    }

    // =========================
    // DETECTAR ENTRADA
    // =========================

    private void DetectarEntrada()
    {
        // Primero comprobamos teclado
        if (Keyboard.current != null)
        {
            foreach (KeyControl tecla in Keyboard.current.allKeys)
            {
                if (tecla == null)
                    continue;

                if (tecla.wasPressedThisFrame)
                {
                    GuardarEntradaTeclado(tecla.keyCode);
                    return;
                }
            }
        }

        // Después comprobamos mouse
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GuardarEntradaMouse(
                    ConfiguracionControles.BotonMouse.Izquierdo
                );

                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                GuardarEntradaMouse(
                    ConfiguracionControles.BotonMouse.Derecho
                );

                return;
            }

            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                GuardarEntradaMouse(
                    ConfiguracionControles.BotonMouse.Medio
                );

                return;
            }
        }
    }

    // =========================
    // GUARDAR TECLADO
    // =========================

    private void GuardarEntradaTeclado(Key tecla)
    {
        switch (controlEsperando)
        {
            case ControlEsperando.Disparo:

                ConfiguracionControles.GuardarDisparoTeclado(tecla);
                textoDisparo.text = tecla.ToString();
                break;

            case ControlEsperando.Esquive:

                ConfiguracionControles.GuardarDash(tecla);
                textoEsquive.text = tecla.ToString();
                break;

            case ControlEsperando.Salto:

                ConfiguracionControles.GuardarSalto(tecla);
                textoSalto.text = tecla.ToString();
                break;

            case ControlEsperando.Sprint:

                ConfiguracionControles.GuardarSprint(tecla);
                textoSprint.text = tecla.ToString();
                break;

            case ControlEsperando.Adelante:

                ConfiguracionControles.GuardarAdelante(tecla);
                textoAdelante.text = tecla.ToString();
                break;

            case ControlEsperando.Izquierda:

                ConfiguracionControles.GuardarIzquierda(tecla);
                textoIzquierda.text = tecla.ToString();
                break;

            case ControlEsperando.Atras:

                ConfiguracionControles.GuardarAtras(tecla);
                textoAtras.text = tecla.ToString();
                break;

            case ControlEsperando.Derecha:

                ConfiguracionControles.GuardarDerecha(tecla);
                textoDerecha.text = tecla.ToString();
                break;
        }

        controlEsperando = ControlEsperando.Ninguno;
    }

    // =========================
    // GUARDAR MOUSE
    // =========================

    private void GuardarEntradaMouse(
        ConfiguracionControles.BotonMouse boton)
    {
        if (controlEsperando != ControlEsperando.Disparo)
            return;

        ConfiguracionControles.GuardarDisparoMouse(boton);

        textoDisparo.text = ObtenerNombreBotonMouse(boton);

        controlEsperando = ControlEsperando.Ninguno;
    }

    // =========================
    // NOMBRE DEL BOTÓN
    // =========================

    private string ObtenerNombreBotonMouse(
        ConfiguracionControles.BotonMouse boton)
    {
        switch (boton)
        {
            case ConfiguracionControles.BotonMouse.Izquierdo:
                return "Click izquierdo";

            case ConfiguracionControles.BotonMouse.Derecho:
                return "Click derecho";

            case ConfiguracionControles.BotonMouse.Medio:
                return "Click central";

            default:
                return "Click izquierdo";
        }
    }

    // =========================
    // ACTUALIZAR TEXTOS
    // =========================

    private void ActualizarTextos()
    {
        textoDisparo.text = ObtenerTextoDisparo();

        textoEsquive.text =
            ConfiguracionControles.ObtenerDash().ToString();

        textoSalto.text =
            ConfiguracionControles.ObtenerSalto().ToString();

        textoSprint.text =
            ConfiguracionControles.ObtenerSprint().ToString();

        textoAdelante.text =
            ConfiguracionControles.ObtenerAdelante().ToString();

        textoIzquierda.text =
            ConfiguracionControles.ObtenerIzquierda().ToString();

        textoAtras.text =
            ConfiguracionControles.ObtenerAtras().ToString();

        textoDerecha.text =
            ConfiguracionControles.ObtenerDerecha().ToString();
    }

    private string ObtenerTextoDisparo()
    {
        if (ConfiguracionControles.ObtenerTipoDisparo()
            == ConfiguracionControles.TipoEntrada.Teclado)
        {
            return ConfiguracionControles
                .ObtenerDisparoTeclado()
                .ToString();
        }

        return ObtenerNombreBotonMouse(
            ConfiguracionControles.ObtenerDisparoMouse()
        );
    }

    // =========================
    // RESTAURAR PREDETERMINADOS
    // =========================

    private void RestaurarPredeterminados()
    {
        ConfiguracionControles.RestaurarPredeterminados();

        ActualizarTextos();
    }
}