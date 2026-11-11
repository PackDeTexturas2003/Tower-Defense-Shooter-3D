
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Menu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuPrincipal");
    }

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

    public void Configuraciones()
    {
        Time.timeScale = 1f;

        Scene escenaNivel =
            SceneManager.GetSceneByName("Nivel");

        Scene escenaConfiguraciones =
            SceneManager.GetSceneByName("Configuraciones");

        if (escenaConfiguraciones.isLoaded)
            return;

        if (escenaNivel.isLoaded)
        {
            SceneManager.LoadSceneAsync(
                "Configuraciones",
                LoadSceneMode.Additive
            );
        }
        else
        {
            SceneManager.LoadScene("Configuraciones");
        }
    }

    public void VolverConfiguraciones()
    {
        Scene escenaNivel =
            SceneManager.GetSceneByName("Nivel");

        if (escenaNivel.isLoaded)
        {
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
            };
        }
        else
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    public void Creditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
