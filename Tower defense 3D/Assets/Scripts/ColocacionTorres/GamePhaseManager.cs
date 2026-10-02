using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    [Header("Cámaras")]
    [SerializeField] private GameObject camaraPlanificacion;
    [SerializeField] private GameObject camaraFPS;

    [Header("Scripts del jugador")]
    [SerializeField] private Movimiento movimiento;
    [SerializeField] private Esquive esquive;
    [SerializeField] private Salto salto;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private CameraEffects cameraEffects;

    [Header("Sistema de torretas")]
    [SerializeField] private ColocacionTorretas colocacionTorretas;

    private bool fasePlanificacion = true;

    private void Start()
    {
        IniciarPlanificacion();
    }

    private void IniciarPlanificacion()
    {
        fasePlanificacion = true;

        // Activar cámara de planificación
        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(true);

        // Desactivar cámara FPS
        if (camaraFPS != null)
            camaraFPS.SetActive(false);

        // Desactivar controles del jugador
        if (movimiento != null)
            movimiento.enabled = false;

        if (esquive != null)
            esquive.enabled = false;

        if (salto != null)
            salto.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;

        if (weaponManager != null)
            weaponManager.enabled = false;

        if (cameraEffects != null)
            cameraEffects.enabled = false;

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ConfirmarPlanificacion()
    {
        if (!fasePlanificacion)
            return;

        fasePlanificacion = false;

        // Detener colocación de torretas
        if (colocacionTorretas != null)
            colocacionTorretas.DetenerColocacion();

        // Desactivar cámara de planificación
        if (camaraPlanificacion != null)
            camaraPlanificacion.SetActive(false);

        // Activar cámara FPS
        if (camaraFPS != null)
            camaraFPS.SetActive(true);

        // Activar controles del jugador
        if (movimiento != null)
            movimiento.enabled = true;

        if (esquive != null)
            esquive.enabled = true;

        if (salto != null)
            salto.enabled = true;

        if (mouseLook != null)
            mouseLook.enabled = true;

        if (weaponManager != null)
            weaponManager.enabled = true;

        if (cameraEffects != null)
        {
            cameraEffects.enabled = true;
            cameraEffects.ReiniciarRecoil();
        }

        // Ocultar y bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (!fasePlanificacion)
            return;

        // Mantener cursor libre durante planificación
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}