using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelVictoriaManager : MonoBehaviour
{
    [Header("Panel de victoria")]
    [SerializeField] private GameObject panelVictoria;

    [Header("Elementos del HUD que se ocultan al ganar")]
    [SerializeField] private GameObject[] elementosHudAOcultar;

    [Header("Control de armas")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Música")]
    [SerializeField] private AmbientMusicManager ambientMusicManager;
    [SerializeField] private AudioClip musicaVictoria;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MenuPrincipal";
    [SerializeField] private string siguienteNivel = "Nivel2";

    private bool victoriaMostrada;

    private void Awake()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PanelVictoriaManager: Asigna el panel de victoria.");
        }

        if (ambientMusicManager == null)
        {
            ambientMusicManager = FindFirstObjectByType<AmbientMusicManager>();
        }

        if (weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<WeaponManager>();
        }
    }

    public void MostrarPanelVictoria()
    {
        if (victoriaMostrada)
            return;

        // Evita que la victoria se procese más de una vez.
        victoriaMostrada = true;

        // Bloquear inmediatamente el control de disparo y cambio de armas.
        if (weaponManager != null)
        {
            weaponManager.enabled = false;
        }
        else
        {
            Debug.LogWarning("PanelVictoriaManager: No se encontró WeaponManager. Asigna la referencia en el Inspector.");
        }

        // Confirmar el progreso del nivel únicamente al ganar.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ConfirmarVictoria();
            SaveManager.Instance.GuardarNivelContinuar(siguienteNivel);
        }

        // Ocultar elementos del HUD.
        if (elementosHudAOcultar != null)
        {
            foreach (GameObject elemento in elementosHudAOcultar)
            {
                if (elemento != null)
                    elemento.SetActive(false);
            }
        }

        ReproducirMusicaVictoria();

        // Mostrar el panel de victoria.
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        // Congelar el juego y liberar el cursor.
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Victoria: progreso confirmado y disparos bloqueados.");
    }

    private void ReproducirMusicaVictoria()
    {
        if (ambientMusicManager == null)
        {
            Debug.LogWarning("PanelVictoriaManager: No se encontró AmbientMusicManager.");
            return;
        }

        ambientMusicManager.DetenerMusica();

        if (musicaVictoria != null)
        {
            ambientMusicManager.ReproducirMusica(musicaVictoria, true);
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Salir al menú desde victoria conservando el progreso confirmado.
    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Pasar directamente al siguiente nivel.
    public void SiguienteNivel()
    {
        if (string.IsNullOrWhiteSpace(siguienteNivel))
        {
            Debug.LogWarning("PanelVictoriaManager: No se asignó el siguiente nivel.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(siguienteNivel))
        {
            Debug.LogError("PanelVictoriaManager: No se encuentra la escena: " + siguienteNivel);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(siguienteNivel);
    }
}
