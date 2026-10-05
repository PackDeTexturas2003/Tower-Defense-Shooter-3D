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

    [Header("Interfaz")]
    [SerializeField] private GameObject botonContinuar;

    [Header("Imágenes de selección de torres")]
    [SerializeField] private Image[] imagenesTorres;

    private bool fasePlanificacion;
    private bool transicionRealizada;

    private float escalaTiempoOriginal;

    private void Start()
    {
        escalaTiempoOriginal = Time.timeScale;

        ConfigurarBotonContinuar();
        IniciarPlanificacion();
    }

    private void ConfigurarBotonContinuar()
    {
        if (botonContinuar == null)
        {
            Debug.LogError("GamePhaseManager: No se asignó el botón Continuar.");
            return;
        }

        Button boton = botonContinuar.GetComponent<Button>();

        if (boton == null)
        {
            Debug.LogError("GamePhaseManager: El objeto asignado como botón Continuar no tiene un componente Button.");
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

        CongelarGameplay();

        if (colocacionTorretas != null)
        {
            colocacionTorretas.enabled = true;
            colocacionTorretas.IniciarColocacion();
        }

        if (botonContinuar != null)
            botonContinuar.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("GamePhaseManager: Fase de planificación iniciada.");
    }

    private void ActivarCamaraPlanificacion()
    {
        if (camaraFPS != null)
            camaraFPS.SetActive(false);

        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(true);
    }

    private void CongelarGameplay()
    {
        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int cantidadCongelada = 0;

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
                continue;

            if (!script.enabled)
                continue;

            if (DebePermanecerActivoDurantePlanificacion(script))
                continue;

            script.enabled = false;
            cantidadCongelada++;
        }

        Debug.Log(
            "GamePhaseManager: Se desactivaron " +
            cantidadCongelada +
            " scripts para la planificación."
        );
    }

    private bool DebePermanecerActivoDurantePlanificacion(MonoBehaviour script)
    {
        if (script == this)
            return true;

        if (script == colocacionTorretas)
            return true;

        if (script is EventSystem)
            return true;

        if (script is BaseInputModule)
            return true;

        if (script is GraphicRaycaster)
            return true;

        if (script is Selectable)
            return true;

        if (botonContinuar != null)
        {
            Transform transformScript = script.transform;
            Transform transformBoton = botonContinuar.transform;

            if (transformScript == transformBoton)
                return true;

            if (transformScript.IsChildOf(transformBoton))
                return true;
        }

        if (imagenesTorres != null)
        {
            for (int i = 0; i < imagenesTorres.Length; i++)
            {
                if (imagenesTorres[i] == null)
                    continue;

                Transform transformImagen = imagenesTorres[i].transform;

                if (script.transform == transformImagen)
                    return true;

                if (script.transform.IsChildOf(transformImagen))
                    return true;
            }
        }

        return false;
    }

    public void ConfirmarPlanificacion()
    {
        Debug.Log("GamePhaseManager: Se recibió el clic de Continuar.");

        if (!fasePlanificacion)
        {
            Debug.LogWarning("GamePhaseManager: La fase de planificación ya terminó.");
            return;
        }

        if (transicionRealizada)
        {
            Debug.LogWarning("GamePhaseManager: La transición ya fue realizada.");
            return;
        }

        transicionRealizada = true;
        fasePlanificacion = false;

        DetenerColocacion();

        if (botonContinuar != null)
            botonContinuar.SetActive(false);

        CambiarACamaraFPS();

        Time.timeScale = escalaTiempoOriginal;

        ReactivarGameplay();

        IniciarOleadas();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("GamePhaseManager: Fase de combate iniciada.");
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
        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int cantidadReactivada = 0;

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
                continue;

            if (script == this)
                continue;

            if (script == colocacionTorretas)
                continue;

            if (script is EventSystem)
                continue;

            if (script is BaseInputModule)
                continue;

            if (script is GraphicRaycaster)
                continue;

            if (script is Selectable)
                continue;

            if (botonContinuar != null)
            {
                Transform transformScript = script.transform;
                Transform transformBoton = botonContinuar.transform;

                if (transformScript == transformBoton)
                    continue;

                if (transformScript.IsChildOf(transformBoton))
                    continue;
            }

            if (!script.enabled)
            {
                script.enabled = true;
                cantidadReactivada++;
            }
        }

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
            Debug.LogError("GamePhaseManager: No se asignó el WaveManager.");
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