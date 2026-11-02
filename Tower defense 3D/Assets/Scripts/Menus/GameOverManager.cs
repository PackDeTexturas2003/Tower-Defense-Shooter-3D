using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public enum CausaDerrota
    {
        JugadorEliminado,
        ObjetivoDestruido
    }

    [Header("Panel Game Over")]
    [SerializeField] private GameObject panelGameOver;

    [Header("Textos")]
    [SerializeField] private TMP_Text tituloGameOver;
    [SerializeField] private TMP_Text mensajeGameOver;

    [Header("Elementos del HUD que se ocultan")]
    [SerializeField] private GameObject[] elementosHudAOcultar;

    [Header("Música")]
    [SerializeField] private AmbientMusicManager ambientMusicManager;
    [SerializeField] private AudioClip musicaDerrota;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MenuPrincipal";

    private bool derrotaMostrada;

    private void Awake()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "GameOverManager: Asigna el panel Game Over."
            );
        }

        if (ambientMusicManager == null)
        {
            ambientMusicManager =
                FindFirstObjectByType<AmbientMusicManager>();
        }
    }

    public void MostrarDerrota(CausaDerrota causa)
    {
        if (derrotaMostrada)
            return;

        derrotaMostrada = true;

        // Cambiar los textos según la causa de derrota.
        if (tituloGameOver != null)
        {
            tituloGameOver.text = "GAME OVER";
        }

        if (mensajeGameOver != null)
        {
            switch (causa)
            {
                case CausaDerrota.JugadorEliminado:
                    mensajeGameOver.text =
                        "El jugador ha sido eliminado.";
                    break;

                case CausaDerrota.ObjetivoDestruido:
                    mensajeGameOver.text =
                        "El objetivo ha sido destruido.";
                    break;
            }
        }

        // Ocultar HUD.
        if (elementosHudAOcultar != null)
        {
            foreach (GameObject elemento in elementosHudAOcultar)
            {
                if (elemento != null)
                    elemento.SetActive(false);
            }
        }

        // Detener música actual y reproducir derrota.
        ReproducirMusicaDerrota();

        // Mostrar panel.
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // Detener el juego.
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "GameOverManager: Derrota mostrada. Causa: " + causa
        );
    }

    private void ReproducirMusicaDerrota()
    {
        if (ambientMusicManager == null)
        {
            Debug.LogWarning(
                "GameOverManager: No se encontró AmbientMusicManager."
            );

            return;
        }

        ambientMusicManager.DetenerMusica();

        if (musicaDerrota != null)
        {
            ambientMusicManager.ReproducirMusica(
                musicaDerrota,
                true
            );
        }
        else
        {
            Debug.LogWarning(
                "GameOverManager: No se asignó la música de derrota."
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
}