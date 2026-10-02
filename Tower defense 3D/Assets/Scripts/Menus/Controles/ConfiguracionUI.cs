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

    [Header("Botones")]
    [SerializeField] private Button botonDisparo;
    [SerializeField] private Button botonEsquive;
    [SerializeField] private Button botonSalto;
    [SerializeField] private Button botonSprint;
    [SerializeField] private Button botonRestaurarPredeterminados;

    private enum ControlEsperando
    {
        Ninguno,
        Disparo,
        Esquive,
        Salto,
        Sprint
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