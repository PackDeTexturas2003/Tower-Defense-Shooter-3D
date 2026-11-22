using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelVictoriaManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelResultado;

    [Header("Textos del resultado")]
    [SerializeField] private TMP_Text textoVidaRestante;
    [SerializeField] private TMP_Text textoTorretasColocadas;
    [SerializeField] private TMP_Text textoTorretasSupervivientes;
    [SerializeField] private TMP_Text textoRecompensaVida;
    [SerializeField] private TMP_Text textoDevolucionTorretas;
    [SerializeField] private TMP_Text textoTotalRecompensa;

    [Header("Boton del resultado")]
    [SerializeField] private Button botonContinuarResultado;

    [Header("Recompensas")]
    [Tooltip("Dinero que se entrega si el jugador termina con toda su vida.")]
    [SerializeField, Min(0)] private int recompensaVidaCompleta = 500;
    [Tooltip("Porcentaje del precio original que se devuelve por cada torreta superviviente.")]
    [SerializeField, Range(0f, 100f)] private float porcentajeDevolucionTorretas = 50f;

    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private DineroJugador dineroJugador;
    [SerializeField] private WeaponManager weaponManager;

    [Header("Elementos del HUD que se ocultan al ganar")]
    [SerializeField] private GameObject[] elementosHudAOcultar;

    [Header("Musica")]
    [SerializeField] private AmbientMusicManager ambientMusicManager;
    [SerializeField] private AudioClip musicaVictoria;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MenuPrincipal";
    [SerializeField] private string siguienteNivel = "Nivel2";

    private bool victoriaMostrada;

    private void Awake()
    {
        if (panelVictoria != null)
            panelVictoria.SetActive(false);
        else
            Debug.LogWarning("PanelVictoriaManager: Asigna el panel de victoria.");

        if (panelResultado != null)
            panelResultado.SetActive(false);

        if (ambientMusicManager == null)
            ambientMusicManager = FindFirstObjectByType<AmbientMusicManager>();

        if (weaponManager == null)
            weaponManager = FindFirstObjectByType<WeaponManager>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (dineroJugador == null)
            dineroJugador = FindFirstObjectByType<DineroJugador>();

        if (botonContinuarResultado != null)
        {
            botonContinuarResultado.onClick.RemoveListener(CerrarPanelResultado);
            botonContinuarResultado.onClick.AddListener(CerrarPanelResultado);
        }
    }

    public void MostrarPanelVictoria()
    {
        if (victoriaMostrada)
            return;

        victoriaMostrada = true;

        if (weaponManager != null)
            weaponManager.enabled = false;

        if (elementosHudAOcultar != null)
        {
            foreach (GameObject elemento in elementosHudAOcultar)
            {
                if (elemento != null)
                    elemento.SetActive(false);
            }
        }

        ReproducirMusicaVictoria();

        // Calcular y entregar las recompensas antes de confirmar el guardado.
        CalcularYEntregarRecompensas();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ConfirmarVictoria();
            SaveManager.Instance.GuardarNivelContinuar(siguienteNivel);
        }

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        // El panel de resultado se muestra encima del panel de victoria.
        if (panelResultado != null)
        {
            panelResultado.SetActive(true);
            panelResultado.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("PanelVictoriaManager: No asignaste Panel Resultado.");
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Victoria: resultado calculado, recompensas entregadas y progreso guardado.");
    }

    private void CalcularYEntregarRecompensas()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (dineroJugador == null)
            dineroJugador = FindFirstObjectByType<DineroJugador>();

        float vidaActual = 0f;
        float vidaMaxima = 0f;
        float porcentajeVida = 0f;

        if (playerHealth != null)
        {
            vidaActual = playerHealth.VidaActual;
            vidaMaxima = playerHealth.VidaMaxima;

            if (vidaMaxima > 0f)
                porcentajeVida = Mathf.Clamp01(vidaActual / vidaMaxima);
        }
        else
        {
            Debug.LogWarning("PanelVictoriaManager: No se encontró PlayerHealth; la recompensa por vida será 0.");
        }

        int recompensaPorVida = Mathf.RoundToInt(
            recompensaVidaCompleta * porcentajeVida
        );

        RegistroTorretasNivel.ObtenerResumen(
            porcentajeDevolucionTorretas,
            out int torretasColocadas,
            out int torretasSupervivientes,
            out int devolucionTorretas
        );

        int recompensaTotal = recompensaPorVida + devolucionTorretas;

        ActualizarTextosResultado(
            vidaActual,
            vidaMaxima,
            torretasColocadas,
            torretasSupervivientes,
            recompensaPorVida,
            devolucionTorretas,
            recompensaTotal
        );

        if (dineroJugador != null && recompensaTotal > 0)
        {
            dineroJugador.AgregarDinero(recompensaTotal);
        }
        else if (dineroJugador == null)
        {
            Debug.LogWarning("PanelVictoriaManager: No se encontró DineroJugador. No se pudo entregar la recompensa.");
        }

        Debug.Log(
            "Resultado de victoria | Vida: " + vidaActual + "/" + vidaMaxima +
            " | Torretas: " + torretasSupervivientes + "/" + torretasColocadas +
            " | Recompensa vida: $" + recompensaPorVida +
            " | Devolución torretas: $" + devolucionTorretas +
            " | Total: $" + recompensaTotal
        );
    }

    private void ActualizarTextosResultado(
        float vidaActual,
        float vidaMaxima,
        int torretasColocadas,
        int torretasSupervivientes,
        int recompensaPorVida,
        int devolucionTorretas,
        int recompensaTotal)
    {
        if (textoVidaRestante != null)
            textoVidaRestante.text = "Vida restante: " + Mathf.CeilToInt(vidaActual) + " / " + Mathf.CeilToInt(vidaMaxima);

        if (textoTorretasColocadas != null)
            textoTorretasColocadas.text = "Torretas colocadas: " + torretasColocadas;

        if (textoTorretasSupervivientes != null)
            textoTorretasSupervivientes.text = "Torretas supervivientes: " + torretasSupervivientes;

        if (textoRecompensaVida != null)
            textoRecompensaVida.text = "Recompensa por vida: $" + recompensaPorVida;

        if (textoDevolucionTorretas != null)
            textoDevolucionTorretas.text = "Devolucion de torretas: $" + devolucionTorretas;

        if (textoTotalRecompensa != null)
            textoTotalRecompensa.text = "TOTAL GANADO: $" + recompensaTotal;
    }

    // El boton Continuar del panel Resultado llama a este metodo.
    // Oculta el resumen y deja visible el panel de victoria que esta debajo.
    public void CerrarPanelResultado()
    {
        if (panelResultado != null)
            panelResultado.SetActive(false);
    }

    private void ReproducirMusicaVictoria()
    {
        if (ambientMusicManager == null)
        {
            Debug.LogWarning("PanelVictoriaManager: No se encontro AmbientMusicManager.");
            return;
        }

        ambientMusicManager.DetenerMusica();

        if (musicaVictoria != null)
            ambientMusicManager.ReproducirMusica(musicaVictoria, true);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            Debug.LogWarning("PanelVictoriaManager: No se asigno el siguiente nivel.");
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
