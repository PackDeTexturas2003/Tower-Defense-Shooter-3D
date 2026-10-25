
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class GamePhaseManager : MonoBehaviour
{
    [Header("Selección inicial de armas")]
    [SerializeField] private GameObject panelSeleccionArmas;

    [Header("Cámaras")]
    [SerializeField] private GameObject camaraPlanificacion;
    [SerializeField] private GameObject camaraFPS;

    [Header("Sistema de torretas")]
    [SerializeField] private ColocacionTorretas colocacionTorretas;

    [Header("Sistema de oleadas")]
    [SerializeField] private WaveManager waveManager;

    [Header("Indicadores de aparición enemiga")]
    [SerializeField] private IndicadorPuntoSpawn[] indicadoresPuntosSpawn;

    [Header("Música de planificación")]
    [SerializeField] private AudioSource audioPlanificacion;

    [Header("Efectos de sonido")]
    [SerializeField] private SFXManager sfxManager;

    [Header("Interfaz")]
    [SerializeField] private GameObject botonContinuar;

    [Header("HUD Planificación")]
    [SerializeField] private GameObject[] elementosHUDPlanificacion;

    [Header("HUD Combate")]
    [SerializeField] private GameObject[] elementosHUDCombate;

    [Header("HUD Compartido")]
    [SerializeField] private GameObject[] elementosHUDCompartido;

    [Header("Imágenes de selección de torres")]
    [SerializeField] private Image[] imagenesTorres;

    [Header("Excepciones de planificación")]
    [SerializeField] private GameObject[] excepcionesPlanificacion;

    private bool faseSeleccionArmas;
    private bool fasePlanificacion;
    private bool transicionRealizada;
    private bool gameplayCongelado;
    private float escalaTiempoOriginal;

    private readonly HashSet<MonoBehaviour> scriptsCongelados =
        new HashSet<MonoBehaviour>();

    private void Start()
    {
        escalaTiempoOriginal = Time.timeScale;

        ConfigurarMusicaPlanificacion();
        ConfigurarBotonContinuar();

        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();

        if (panelSeleccionArmas != null &&
            panelSeleccionArmas.activeInHierarchy)
        {
            IniciarSeleccionArmas();
        }
        else
        {
            IniciarPlanificacion();
        }
    }

    private void ConfigurarMusicaPlanificacion()
    {
        if (audioPlanificacion == null)
            return;

        audioPlanificacion.playOnAwake = false;
        audioPlanificacion.loop = true;
        audioPlanificacion.spatialBlend = 0f;
    }

    private void ConfigurarBotonContinuar()
    {
        if (botonContinuar == null)
        {
            Debug.LogError(
                "GamePhaseManager: No se asignó el botón Continuar."
            );
            return;
        }

        Button boton = botonContinuar.GetComponent<Button>();

        if (boton == null)
        {
            Debug.LogError(
                "GamePhaseManager: El botón Continuar no tiene Button."
            );
            return;
        }

        boton.onClick.RemoveListener(ConfirmarPlanificacion);
        boton.onClick.AddListener(ConfirmarPlanificacion);
    }

    private void IniciarSeleccionArmas()
    {
        faseSeleccionArmas = true;
        fasePlanificacion = false;
        transicionRealizada = false;

        Time.timeScale = 0f;

        if (panelSeleccionArmas != null)
            panelSeleccionArmas.SetActive(true);

        // La cámara FPS permanece activa para renderizar
        // la escena y conservar su Audio Listener.
        if (camaraFPS != null)
            camaraFPS.SetActive(true);

        // Ocultamos la cámara de planificación, no la FPS.
        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(false);

        if (botonContinuar != null)
            botonContinuar.SetActive(false);

        CambiarEstadoElementos(elementosHUDPlanificacion, false);
        CambiarEstadoElementos(elementosHUDCombate, false);

        if (colocacionTorretas != null)
            colocacionTorretas.enabled = false;

        CongelarGameplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "GamePhaseManager: Selección de armas iniciada."
        );
    }

    public void ConfirmarSeleccionArmas()
    {
        if (!faseSeleccionArmas)
        {
            Debug.LogWarning(
                "GamePhaseManager: No estamos en la selección de armas."
            );
            return;
        }

        faseSeleccionArmas = false;

        if (panelSeleccionArmas != null)
            panelSeleccionArmas.SetActive(false);

        Debug.Log(
            "GamePhaseManager: Selección de armas completada."
        );

        IniciarPlanificacion();
    }

    private void IniciarPlanificacion()
    {
        faseSeleccionArmas = false;
        fasePlanificacion = true;
        transicionRealizada = false;

        Time.timeScale = 0f;

        if (panelSeleccionArmas != null)
            panelSeleccionArmas.SetActive(false);

        ActivarCamaraPlanificacion();
        ActivarHUDPlanificacion();

        if (!gameplayCongelado)
            CongelarGameplay();

        MostrarIndicadoresSpawn();

        if (colocacionTorretas != null)
        {
            colocacionTorretas.enabled = true;
            colocacionTorretas.IniciarColocacion();
        }

        if (botonContinuar != null)
            botonContinuar.SetActive(true);

        ReproducirMusicaPlanificacion();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "GamePhaseManager: Fase de planificación iniciada."
        );
    }

    private void MostrarIndicadoresSpawn()
    {
        if (indicadoresPuntosSpawn == null)
            return;

        foreach (IndicadorPuntoSpawn indicador in indicadoresPuntosSpawn)
        {
            if (indicador == null)
                continue;

            indicador.enabled = true;
            indicador.MostrarIndicador();
        }
    }

    private void OcultarIndicadoresSpawn()
    {
        if (indicadoresPuntosSpawn == null)
            return;

        foreach (IndicadorPuntoSpawn indicador in indicadoresPuntosSpawn)
        {
            if (indicador != null)
                indicador.OcultarIndicador();
        }
    }

    private void ReproducirMusicaPlanificacion()
    {
        if (audioPlanificacion == null || audioPlanificacion.clip == null)
            return;

        if (!audioPlanificacion.isPlaying)
            audioPlanificacion.Play();
    }

    private void DetenerMusicaPlanificacion()
    {
        if (audioPlanificacion != null)
            audioPlanificacion.Stop();
    }

    private void ActivarCamaraPlanificacion()
    {
        // Al planificar, se utiliza la cámara de planificación.
        if (camaraFPS != null)
            camaraFPS.SetActive(false);

        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(true);
    }

    private void ActivarHUDPlanificacion()
    {
        CambiarEstadoElementos(elementosHUDPlanificacion, true);
        CambiarEstadoElementos(elementosHUDCombate, false);
        CambiarEstadoElementos(elementosHUDCompartido, true);
    }

    private void ActivarHUDCombate()
    {
        CambiarEstadoElementos(elementosHUDPlanificacion, false);
        CambiarEstadoElementos(elementosHUDCombate, true);
        CambiarEstadoElementos(elementosHUDCompartido, true);
    }

    private void CambiarEstadoElementos(
        GameObject[] elementos,
        bool estado)
    {
        if (elementos == null)
            return;

        foreach (GameObject elemento in elementos)
        {
            if (elemento != null)
                elemento.SetActive(estado);
        }
    }

    private void CongelarGameplay()
    {
        if (gameplayCongelado)
            return;

        scriptsCongelados.Clear();

        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null || !script.enabled)
                continue;

            if (DebePermanecerActivoDurantePlanificacion(script))
                continue;

            script.enabled = false;
            scriptsCongelados.Add(script);
        }

        gameplayCongelado = true;

        Debug.Log(
            "GamePhaseManager: Gameplay congelado durante la preparación."
        );
    }

    private bool DebePermanecerActivoDurantePlanificacion(
        MonoBehaviour script)
    {
        if (script == this ||
            script is SeleccionArmas ||
            script == colocacionTorretas)
            return true;

        if (script is IndicadorPuntoSpawn ||
            script is EventSystem ||
            script is BaseInputModule ||
            script is GraphicRaycaster ||
            script is Selectable)
            return true;

        if (PerteneceAObjeto(script, panelSeleccionArmas) ||
            PerteneceAObjeto(script, botonContinuar))
            return true;

        if (elementosHUDCompartido != null)
        {
            foreach (GameObject elemento in elementosHUDCompartido)
            {
                if (PerteneceAObjeto(script, elemento))
                    return true;
            }
        }

        if (imagenesTorres != null)
        {
            foreach (Image imagen in imagenesTorres)
            {
                if (imagen != null &&
                    PerteneceAObjeto(script, imagen.gameObject))
                    return true;
            }
        }

        if (excepcionesPlanificacion != null)
        {
            foreach (GameObject excepcion in excepcionesPlanificacion)
            {
                if (PerteneceAObjeto(script, excepcion))
                    return true;
            }
        }

        return false;
    }

    private bool PerteneceAObjeto(
        MonoBehaviour script,
        GameObject objeto)
    {
        if (script == null || objeto == null)
            return false;

        Transform tScript = script.transform;
        Transform tObjeto = objeto.transform;

        return tScript == tObjeto || tScript.IsChildOf(tObjeto);
    }

    public void ConfirmarPlanificacion()
    {
        if (!fasePlanificacion || transicionRealizada)
            return;

        transicionRealizada = true;
        fasePlanificacion = false;

        if (sfxManager != null)
            sfxManager.ReproducirContinuar();

        OcultarIndicadoresSpawn();
        DetenerMusicaPlanificacion();
        DetenerColocacion();

        if (botonContinuar != null)
            botonContinuar.SetActive(false);

        CambiarACamaraFPS();
        ActivarHUDCombate();

        Time.timeScale = escalaTiempoOriginal;

        ReactivarGameplay();
        IniciarOleadas();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log(
            "GamePhaseManager: Fase de combate iniciada."
        );
    }

    private void DetenerColocacion()
    {
        if (colocacionTorretas == null)
            return;

        colocacionTorretas.DetenerColocacion();
        colocacionTorretas.enabled = false;
    }

    private void CambiarACamaraFPS()
    {
        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(false);

        if (camaraFPS != null)
            camaraFPS.SetActive(true);
    }

    private void ReactivarGameplay()
    {
        foreach (MonoBehaviour script in scriptsCongelados)
        {
            if (script != null)
                script.enabled = true;
        }

        scriptsCongelados.Clear();
        gameplayCongelado = false;
    }

    private void IniciarOleadas()
    {
        if (waveManager == null)
        {
            Debug.LogError(
                "GamePhaseManager: No se asignó el WaveManager."
            );
            return;
        }

        waveManager.IniciarJuego();

        Debug.Log("GamePhaseManager: Oleadas iniciadas.");
    }

    private void LateUpdate()
    {
        if (faseSeleccionArmas || fasePlanificacion)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = escalaTiempoOriginal;
    }
}
