using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Panel de pausa")]
    [SerializeField] private GameObject panelPausa;

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

        panelPausa.SetActive(false);

        botonContinuar.onClick.AddListener(Continuar);

        botonConfiguraciones.onClick.AddListener(
            IrAConfiguraciones
        );
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (juegoPausado)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }

    // =========================
    // PAUSAR
    // =========================

    private void Pausar()
    {
        juegoPausado = true;

        Time.timeScale = 0f;

        movimiento.enabled = false;
        esquive.enabled = false;
        salto.enabled = false;
        mouseLook.enabled = false;
        weaponManager.enabled = false;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        panelPausa.SetActive(true);
    }

    // =========================
    // CONTINUAR
    // =========================

    private void Continuar()
    {
        juegoPausado = false;

        Time.timeScale = 1f;

        movimiento.enabled = true;
        esquive.enabled = true;
        salto.enabled = true;
        mouseLook.enabled = true;
        weaponManager.enabled = true;

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        panelPausa.SetActive(false);
    }

    // =========================
    // CONFIGURACIONES
    // =========================

    private void IrAConfiguraciones()
    {
        // Ocultamos únicamente el panel de pausa
        panelPausa.SetActive(false);

        // El juego continúa pausado
        Time.timeScale = 0f;

        // Dejamos el cursor libre para utilizar la interfaz
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        // Cargamos Configuraciones sin descargar Nivel
        SceneManager.LoadSceneAsync(
            "Configuraciones",
            LoadSceneMode.Additive
        );
    }
    public void ReanudarDesdeConfiguraciones()
    {
        juegoPausado = false;

        Time.timeScale = 1f;

        movimiento.enabled = true;
        esquive.enabled = true;
        salto.enabled = true;
        mouseLook.enabled = true;
        weaponManager.enabled = true;

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        panelPausa.SetActive(false);
    }
    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            "MenuPrincipal"
        );
    }
}