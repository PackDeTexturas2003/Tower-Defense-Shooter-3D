
using UnityEngine;
using UnityEngine.SceneManagement;

public class CamaraConfiguraciones : MonoBehaviour
{
    private Camera camara;

    private void Awake()
    {
        camara = GetComponent<Camera>();

        Scene escenaNivel =
            SceneManager.GetSceneByName("Nivel");

        if (escenaNivel.isLoaded)
        {
            // Si estamos en el nivel, no renderizar.
            camara.enabled = false;
        }
        else
        {
            // Si estamos en el menú, renderizar.
            camara.enabled = true;
        }
    }
}
