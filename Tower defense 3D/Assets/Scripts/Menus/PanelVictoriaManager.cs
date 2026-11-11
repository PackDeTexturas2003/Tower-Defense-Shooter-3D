
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelVictoriaManager : MonoBehaviour
{
    [Header("Panel de victoria")]
    [SerializeField] private GameObject panelVictoria;

    [Header("Elementos del HUD que se ocultan al ganar")]
    [SerializeField] private GameObject[] elementosHudAOcultar;

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
            Debug.LogWarning(
                "PanelVictoriaManager: Asigna el panel de victoria."
            );
        }

        if (ambientMusicManager == null)
        {
            ambientMusicManager =
                FindFirstObjectByType<AmbientMusicManager>();
        }
    }

    public void MostrarPanelVictoria()
    {
        if (victoriaMostrada)
            return;

        victoriaMostrada = true;

        // Confirmar el progreso del nivel.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ConfirmarVictoria();

            // Guardar el siguiente nivel automáticamente.
            SaveManager.Instance.GuardarNivelContinuar(
                siguienteNivel
            );
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

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "Victoria: siguiente nivel guardado: " + siguienteNivel
        );
    }

    private void ReproducirMusicaVictoria()
    {
        if (ambientMusicManager == null)
        {
            Debug.LogWarning(
                "No se encontró AmbientMusicManager."
            );

            return;
        }

        ambientMusicManager.DetenerMusica();

        if (musicaVictoria != null)
        {
            ambientMusicManager.ReproducirMusica(
                musicaVictoria,
                true
            );
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Salir al menú conservando el siguiente nivel.
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
            Debug.LogWarning(
                "No se asignó el siguiente nivel."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(siguienteNivel))
        {
            Debug.LogError(
                "No se encuentra la escena: " + siguienteNivel
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(siguienteNivel);
    }
}
