
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelVictoriaManager : MonoBehaviour
{
    [Header("Panel de victoria")]
    [SerializeField] private GameObject panelVictoria;

    [Header("Elementos del HUD que se ocultan al ganar")]
    [SerializeField] private GameObject[] elementosHudAOcultar;

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
    }

    public void MostrarPanelVictoria()
    {
        if (victoriaMostrada)
            return;

        victoriaMostrada = true;

        // Ocultar los elementos del HUD.
        if (elementosHudAOcultar != null)
        {
            foreach (GameObject elemento in elementosHudAOcultar)
            {
                if (elemento != null)
                    elemento.SetActive(false);
            }
        }

        // Mostrar el panel de victoria.
        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        // Pausar el juego y liberar el cursor.
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "PanelVictoriaManager: ¡Victoria! HUD oculto y panel mostrado."
        );
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

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

    public void SiguienteNivel()
    {
        if (string.IsNullOrWhiteSpace(siguienteNivel))
        {
            Debug.LogWarning(
                "PanelVictoriaManager: Escribe el nombre de la escena siguiente."
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(siguienteNivel))
        {
            Debug.LogError(
                "No se puede cargar la escena '" + siguienteNivel +
                "'. Comprueba que exista y esté incluida en Build Profiles."
            );
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(siguienteNivel);
    }
}
