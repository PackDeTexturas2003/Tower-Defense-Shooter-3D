using UnityEngine;
using UnityEngine.InputSystem;

public static class ConfiguracionControles
{
    // =========================
    // CLAVES PLAYERPREFS
    // =========================

    private const string TECLA_DISPARO = "TeclaDisparo";
    private const string TIPO_DISPARO = "TipoDisparo";

    private const string TECLA_DASH = "TeclaDash";
    private const string TECLA_SALTO = "TeclaSalto";
    private const string TECLA_SPRINT = "TeclaSprint";


    // =========================
    // TIPOS DE DISPARO
    // =========================

    public enum TipoEntrada
    {
        Mouse = 0,
        Teclado = 1
    }


    public enum BotonMouse
    {
        Izquierdo = 0,
        Derecho = 1,
        Medio = 2
    }


    // =========================
    // VALORES PREDETERMINADOS
    // =========================

    private const BotonMouse DISPARO_MOUSE_DEFAULT =
        BotonMouse.Izquierdo;

    private const Key DISPARO_TECLADO_DEFAULT =
        Key.Space;

    private const Key DASH_DEFAULT =
        Key.E;

    private const Key SALTO_DEFAULT =
        Key.Space;

    private const Key SPRINT_DEFAULT =
        Key.LeftShift;


    // =========================
    // GUARDAR DISPARO
    // =========================

    public static void GuardarDisparoMouse(BotonMouse boton)
    {
        PlayerPrefs.SetInt(
            TIPO_DISPARO,
            (int)TipoEntrada.Mouse
        );

        PlayerPrefs.SetInt(
            TECLA_DISPARO,
            (int)boton
        );

        PlayerPrefs.Save();
    }

    public static void GuardarDisparoTeclado(Key tecla)
    {
        PlayerPrefs.SetInt(
            TIPO_DISPARO,
            (int)TipoEntrada.Teclado
        );

        PlayerPrefs.SetInt(
            TECLA_DISPARO,
            (int)tecla
        );

        PlayerPrefs.Save();
    }


    // =========================
    // OBTENER DISPARO
    // =========================

    public static TipoEntrada ObtenerTipoDisparo()
    {
        return (TipoEntrada)PlayerPrefs.GetInt(
            TIPO_DISPARO,
            (int)TipoEntrada.Mouse
        );
    }

    public static BotonMouse ObtenerDisparoMouse()
    {
        return (BotonMouse)PlayerPrefs.GetInt(
            TECLA_DISPARO,
            (int)DISPARO_MOUSE_DEFAULT
        );
    }

    public static Key ObtenerDisparoTeclado()
    {
        return (Key)PlayerPrefs.GetInt(
            TECLA_DISPARO,
            (int)DISPARO_TECLADO_DEFAULT
        );
    }


    // =========================
    // GUARDAR OTROS CONTROLES
    // =========================

    public static void GuardarDash(Key tecla)
    {
        PlayerPrefs.SetInt(
            TECLA_DASH,
            (int)tecla
        );

        PlayerPrefs.Save();
    }

    public static void GuardarSalto(Key tecla)
    {
        PlayerPrefs.SetInt(
            TECLA_SALTO,
            (int)tecla
        );

        PlayerPrefs.Save();
    }

    public static void GuardarSprint(Key tecla)
    {
        PlayerPrefs.SetInt(
            TECLA_SPRINT,
            (int)tecla
        );

        PlayerPrefs.Save();
    }


    // =========================
    // OBTENER OTROS CONTROLES
    // =========================

    public static Key ObtenerDash()
    {
        return (Key)PlayerPrefs.GetInt(
            TECLA_DASH,
            (int)DASH_DEFAULT
        );
    }

    public static Key ObtenerSalto()
    {
        return (Key)PlayerPrefs.GetInt(
            TECLA_SALTO,
            (int)SALTO_DEFAULT
        );
    }

    public static Key ObtenerSprint()
    {
        return (Key)PlayerPrefs.GetInt(
            TECLA_SPRINT,
            (int)SPRINT_DEFAULT
        );
    }


    // =========================
    // RESTAURAR PREDETERMINADOS
    // =========================

    public static void RestaurarPredeterminados()
    {
        PlayerPrefs.SetInt(
            TIPO_DISPARO,
            (int)TipoEntrada.Mouse
        );

        PlayerPrefs.SetInt(
            TECLA_DISPARO,
            (int)DISPARO_MOUSE_DEFAULT
        );

        PlayerPrefs.SetInt(
            TECLA_DASH,
            (int)DASH_DEFAULT
        );

        PlayerPrefs.SetInt(
            TECLA_SALTO,
            (int)SALTO_DEFAULT
        );

        PlayerPrefs.SetInt(
            TECLA_SPRINT,
            (int)SPRINT_DEFAULT
        );

        PlayerPrefs.Save();
    }
}