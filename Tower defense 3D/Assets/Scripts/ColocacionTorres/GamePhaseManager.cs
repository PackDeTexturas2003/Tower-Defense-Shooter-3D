using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class GamePhaseManager : MonoBehaviour
{
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

    private bool fasePlanificacion;
    private bool transicionRealizada;
    private float escalaTiempoOriginal;

    private readonly HashSet<MonoBehaviour> scriptsCongelados =
        new HashSet<MonoBehaviour>();

    private void Start()
    {
        escalaTiempoOriginal = Time.timeScale;

        ConfigurarMusicaPlanificacion();
        ConfigurarBotonContinuar();

        IniciarPlanificacion();
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
                "GamePhaseManager: El objeto asignado como botón Continuar no tiene un componente Button."
            );
            return;
        }

        boton.onClick.AddListener(ConfirmarPlanificacion);
    }

    private void IniciarPlanificacion()
    {
        fasePlanificacion = true;
        transicionRealizada = false;

        Time.timeScale = 0f;

        ActivarCamaraPlanificacion();
        ActivarHUDPlanificacion();
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
            if (indicador == null)
                continue;

            indicador.OcultarIndicador();
        }
    }

    private void ReproducirMusicaPlanificacion()
    {
        if (audioPlanificacion == null)
        {
            Debug.LogWarning(
                "GamePhaseManager: No se asignó el AudioSource de planificación."
            );
            return;
        }

        if (audioPlanificacion.clip == null)
        {
            Debug.LogWarning(
                "GamePhaseManager: El AudioSource de planificación no tiene un AudioClip."
            );
            return;
        }

        audioPlanificacion.Play();
    }

    private void DetenerMusicaPlanificacion()
    {
        if (audioPlanificacion != null)
            audioPlanificacion.Stop();
    }

    private void ActivarCamaraPlanificacion()
    {
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
        scriptsCongelados.Clear();

        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int cantidadCongelada = 0;

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null || !script.enabled)
                continue;

            if (DebePermanecerActivoDurantePlanificacion(script))
                continue;

            script.enabled = false;
            scriptsCongelados.Add(script);
            cantidadCongelada++;
        }

        Debug.Log(
            "GamePhaseManager: Se desactivaron " +
            cantidadCongelada +
            " scripts para la planificación."
        );
    }

    private bool DebePermanecerActivoDurantePlanificacion(
        MonoBehaviour script)
    {
        if (script == this)
            return true;

        if (script == colocacionTorretas)
            return true;

        if (script is IndicadorPuntoSpawn ||
            script is EventSystem ||
            script is BaseInputModule ||
            script is GraphicRaycaster ||
            script is Selectable)
            return true;

        if (PerteneceAObjeto(script, botonContinuar))
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

        Transform transformScript = script.transform;
        Transform transformObjeto = objeto.transform;

        return transformScript == transformObjeto ||
               transformScript.IsChildOf(transformObjeto);
    }

    public void ConfirmarPlanificacion()
    {
        Debug.Log(
            "GamePhaseManager: Se recibió el clic de Continuar."
        );

        if (!fasePlanificacion || transicionRealizada)
        {
            Debug.LogWarning(
                "GamePhaseManager: La planificación ya terminó o la transición ya se realizó."
            );
            return;
        }

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
        int cantidadReactivada = 0;

        foreach (MonoBehaviour script in scriptsCongelados)
        {
            if (script == null || script.enabled)
                continue;

            script.enabled = true;
            cantidadReactivada++;
        }

        scriptsCongelados.Clear();

        Debug.Log(
            "GamePhaseManager: Se reactivaron " +
            cantidadReactivada +
            " scripts para la fase de combate."
        );
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
        if (!fasePlanificacion)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        Time.timeScale = escalaTiempoOriginal;
    }
}
