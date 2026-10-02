using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Menu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void Continuar()
    {
        SceneManager.LoadScene("Nivel");
    }

    public void NuevaPartida()
    {
        SceneManager.LoadScene("Nivel");
    }

    public void Configuraciones()
    {
        Scene escenaNivel =
            SceneManager.GetSceneByName("Nivel");

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