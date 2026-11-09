
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Panel de pausa")]
    [SerializeField] private GameObject panelPausa;

    [Header("Orden visual")]
    [SerializeField] private bool ponerPanelAlFrente = true;

    [Header("Botones")]
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonConfiguraciones;

    [Header("Scripts del jugador")]
    [SerializeField] private Movimiento movimiento;
    [SerializeField] private Esquive esquive;
    [SerializeField] private Salto salto;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private WeaponManager weaponManager;

    [Header("Música")]
    [SerializeField] private AmbientMusicManager ambientMusicManager;

    private bool juegoPausado = false;
    private bool configuracionesAbiertasDesdePausa = false;

    private void Start()
    {
        juegoPausado = false;
        configuracionesAbiertasDesdePausa = false;

        Time.timeScale = 1f;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(Continuar);

        if (botonConfiguraciones != null)
            botonConfiguraciones.onClick.AddListener(IrAConfiguraciones);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (juegoPausado)
        {
            Continuar();
        }
        else
        {
            Pausar();
        }
    }

    // =========================
    // PAUSAR JUEGO
    // =========================

    private void Pausar()
    {
        juegoPausado = true;

        Time.timeScale = 0f;

        if (ambientMusicManager != null)
            ambientMusicManager.PausarMusica();

        if (movimiento != null)
            movimiento.enabled = false;

        if (esquive != null)
            esquive.enabled = false;

        if (salto != null)
            salto.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;

        if (weaponManager != null)
            weaponManager.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            PonerPanelAlFrente();
        }
    }

    // =========================
    // ORDEN VISUAL DEL PANEL
    // =========================

    private void PonerPanelAlFrente()
    {
        if (!ponerPanelAlFrente || panelPausa == null)
            return;

        Canvas canvasPanel = panelPausa.GetComponentInParent<Canvas>();

        if (canvasPanel == null)
            return;

        canvasPanel = canvasPanel.rootCanvas;

        panelPausa.transform.SetAsLastSibling();

        if (canvasPanel.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasPanel.overrideSorting = true;
            canvasPanel.sortingOrder = 100;
        }
    }

    // =========================
    // CONTINUAR JUEGO
    // =========================

    private void Continuar()
    {
        // Si Configuración se abrió desde Pausa,
        // se cierra antes de reanudar el nivel.
        if (configuracionesAbiertasDesdePausa)
        {
            CerrarEscenaConfiguraciones();
            configuracionesAbiertasDesdePausa = false;
        }

        juegoPausado = false;

        Time.timeScale = 1f;

        if (movimiento != null)
            movimiento.enabled = true;

        if (esquive != null)
            esquive.enabled = true;

        if (salto != null)
            salto.enabled = true;

        if (mouseLook != null)
            mouseLook.enabled = true;

        if (weaponManager != null)
            weaponManager.enabled = true;

        if (ambientMusicManager != null)
            ambientMusicManager.ContinuarMusica();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (panelPausa != null)
            panelPausa.SetActive(false);
    }

    // =========================
    // ABRIR CONFIGURACIONES
    // =========================

    private void IrAConfiguraciones()
    {
        configuracionesAbiertasDesdePausa = true;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Scene escenaConfiguraciones =
            SceneManager.GetSceneByName("Configuraciones");

        if (escenaConfiguraciones.isLoaded)
            return;

        SceneManager.LoadSceneAsync(
            "Configuraciones",
            LoadSceneMode.Additive
        );
    }

    // =========================
    // CERRAR CONFIGURACIONES
    // =========================

    private void CerrarEscenaConfiguraciones()
    {
        Scene escenaConfiguraciones =
            SceneManager.GetSceneByName("Configuraciones");

        if (escenaConfiguraciones.isLoaded)
        {
            SceneManager.UnloadSceneAsync("Configuraciones");
        }
    }

    // Este método puede seguir conectado
    // al botón Volver de Configuración.
    public void ReanudarDesdeConfiguraciones()
    {
        Continuar();
    }

    // =========================
    // VOLVER AL MENU PRINCIPAL
    // =========================

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("MenuPrincipal");
    }
}
