
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // =========================
    // DETECTAR SI HAY UN NIVEL CARGADO
    // =========================

    private bool HayNivelCargado()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene escena = SceneManager.GetSceneAt(i);

            if (escena.isLoaded &&
                escena.name.StartsWith("Nivel"))
            {
                return true;
            }
        }

        return false;
    }

    // =========================
    // MENU PRINCIPAL
    // =========================

    public void Menu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuPrincipal");
    }

    // =========================
    // CONTINUAR PARTIDA
    // =========================

    public void Continuar()
    {
        Time.timeScale = 1f;

        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "MenuManager: No se encontró SaveManager."
            );

            return;
        }

        string nivelGuardado =
            SaveManager.Instance.ObtenerNivelContinuar();

        if (!Application.CanStreamedLevelBeLoaded(nivelGuardado))
        {
            Debug.LogError(
                "No se encuentra el nivel guardado: " + nivelGuardado
            );

            return;
        }

        Debug.Log("Continuando desde: " + nivelGuardado);

        SceneManager.LoadScene(nivelGuardado);
    }

    // =========================
    // NUEVA PARTIDA
    // =========================

    public void NuevaPartida()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "MenuManager: No se encontró SaveManager."
            );

            return;
        }

        SaveManager.Instance.NuevaPartida();

        Time.timeScale = 1f;

        SceneManager.LoadScene("Nivel");
    }

    // =========================
    // ABRIR CONFIGURACIONES
    // =========================

    public void Configuraciones()
    {
        Time.timeScale = 1f;

        Scene escenaConfiguraciones =
            SceneManager.GetSceneByName("Configuraciones");

        if (escenaConfiguraciones.isLoaded)
            return;

        if (HayNivelCargado())
        {
            // Si estamos dentro de cualquier nivel,
            // abrir configuraciones sin cerrar el nivel.
            SceneManager.LoadSceneAsync(
                "Configuraciones",
                LoadSceneMode.Additive
            );
        }
        else
        {
            // Si estamos en el menú principal.
            SceneManager.LoadScene("Configuraciones");
        }
    }

    // =========================
    // VOLVER DESDE CONFIGURACIONES
    // =========================

    public void VolverConfiguraciones()
    {
        if (HayNivelCargado())
        {
            // Estamos dentro de un nivel.
            // Cerrar configuraciones y regresar a la pausa.

            Scene escenaConfiguraciones =
                SceneManager.GetSceneByName("Configuraciones");

            if (!escenaConfiguraciones.isLoaded)
            {
                Debug.LogWarning(
                    "MenuManager: La escena Configuraciones no está cargada."
                );

                return;
            }

            SceneManager.UnloadSceneAsync(
                "Configuraciones"
            ).completed += _ =>
            {
                MenuPausa menuPausa =
                    FindFirstObjectByType<MenuPausa>();

                if (menuPausa != null)
                {
                    menuPausa.ReanudarDesdeConfiguraciones();
                }
                else
                {
                    Debug.LogWarning(
                        "MenuManager: No se encontró MenuPausa."
                    );
                }
            };
        }
        else
        {
            // Estamos en configuraciones del menú principal.
            Time.timeScale = 1f;

            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    // =========================
    // CREDITOS
    // =========================

    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    // =========================
    // SALIR DEL JUEGO
    // =========================

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
