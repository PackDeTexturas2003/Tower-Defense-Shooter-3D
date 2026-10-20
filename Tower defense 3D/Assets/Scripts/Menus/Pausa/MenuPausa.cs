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

    private bool juegoPausado = false;

    private void Start()
    {
        juegoPausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(Continuar);

        if (botonConfiguraciones != null)
        {
            botonConfiguraciones.onClick.AddListener(
                IrAConfiguraciones
            );
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (juegoPausado)
            Continuar();
        else
            Pausar();
    }

    private void Pausar()
    {
        juegoPausado = true;
        Time.timeScale = 0f;

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

    private void PonerPanelAlFrente()
    {
        if (!ponerPanelAlFrente || panelPausa == null)
            return;

        Canvas canvasPanel = panelPausa.GetComponentInParent<Canvas>();

        if (canvasPanel == null)
            return;

        canvasPanel = canvasPanel.rootCanvas;

        if (canvasPanel.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            panelPausa.transform.SetAsLastSibling();
        }
        else
        {
            panelPausa.transform.SetAsLastSibling();

            canvasPanel.overrideSorting = true;
            canvasPanel.sortingOrder = 100;
        }
    }

    private void Continuar()
    {
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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (panelPausa != null)
            panelPausa.SetActive(false);
    }

    private void IrAConfiguraciones()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadSceneAsync(
            "Configuraciones",
            LoadSceneMode.Additive
        );
    }

    public void ReanudarDesdeConfiguraciones()
    {
        Continuar();
    }

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("MenuPrincipal");
    }
}