
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
        SceneManager.LoadScene("Nivel");
    }

    public void NuevaPartida()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "MenuManager: No se encontró el SaveManager."
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
        {
            Debug.Log("Configuraciones ya está cargada.");
            return;
        }

        if (escenaNivel.isLoaded)
        {
            // Si estamos en el Nivel, cargar de forma aditiva.
            SceneManager.LoadSceneAsync(
                "Configuraciones",
                LoadSceneMode.Additive
            );
        }
        else
        {
            // Si estamos en el menú, cargar normalmente.
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
        Time.timeScale = 1f;
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
