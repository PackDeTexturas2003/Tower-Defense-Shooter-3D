
using UnityEngine;
using System.Collections;

public enum AmmoType
{
    Pistola,
    Shotgun,
    SMG,
    Rifle,
    Sniper,
    RPG
}

public abstract class Weapon : MonoBehaviour
{
    [Header("Información")]
    public string nombreArma;

    [Header("Tipo de Munición")]
    [SerializeField]
    protected AmmoType tipoMunicion = AmmoType.Pistola;

    public AmmoType TipoMunicion => tipoMunicion;

    [Header("Modo de Disparo")]
    [SerializeField]
    protected FireMode fireMode = FireMode.SemiAuto;

    public FireMode FireMode => fireMode;

    [Header("Disparo")]
    public float daño = 10f;
    public float tiempoEntreDisparos = 1f;
    public float distancia = 100f;

    [Header("Retroceso del jugador")]
    [SerializeField]
    protected float fuerzaRetrocesoJugador = 1f;

    [Header("Empuje al enemigo")]
    [SerializeField]
    protected float fuerzaEmpujeEnemigo = 2f;

    [Header("Munición")]
    [SerializeField]
    protected int capacidadCargador = 12;

    [SerializeField]
    protected int municionInicial = 12;

    [SerializeField]
    protected int reservaMaxima = 60;

    [SerializeField]
    protected int municionReservaInicial = 60;

    [Header("Recarga")]
    [SerializeField]
    protected float tiempoRecarga = 1.5f;

    [Header("Sonido")]
    [SerializeField]
    protected SFXManager sfxManager;

    protected Camera camara;
    protected float siguienteDisparo;
    protected int municionActual;
    protected int municionReserva;
    protected bool recargando;

    protected virtual void Awake()
    {
        camara = Camera.main;

        municionActual = Mathf.Clamp(
            municionInicial,
            0,
            capacidadCargador
        );

        municionReserva = Mathf.Clamp(
            municionReservaInicial,
            0,
            reservaMaxima
        );

        if (sfxManager == null)
        {
            sfxManager = FindFirstObjectByType<SFXManager>();
        }
    }

    protected virtual void Update()
    {
        // Solo se comprueba el botón para la SMG.
        if (tipoMunicion != AmmoType.SMG)
            return;

        // Detiene el audio al soltar el botón,
        // quedarse sin munición o comenzar a recargar.
        if (!Input.GetMouseButton(0) ||
            !TieneMunicion() ||
            recargando)
        {
            DetenerSonidoSMG();
        }
    }

    public virtual void Disparar()
    {
    }

    // =====================================================
    // SONIDO DE LA SMG
    // =====================================================

    protected void IniciarSonidoSMG()
    {
        if (tipoMunicion != AmmoType.SMG)
            return;

        if (sfxManager == null)
            return;

        if (!Input.GetMouseButton(0))
            return;

        if (!TieneMunicion() || recargando)
            return;

        sfxManager.IniciarSonidoSMG();
    }

    protected void DetenerSonidoSMG()
    {
        if (tipoMunicion != AmmoType.SMG)
            return;

        if (sfxManager != null)
        {
            sfxManager.DetenerSonidoSMG();
        }
    }

    // =====================================================
    // MUNICIÓN
    // =====================================================

    public bool TieneMunicion()
    {
        return municionActual > 0;
    }

    protected bool ConsumirMunicion()
    {
        if (municionActual <= 0)
        {
            DetenerSonidoSMG();
            return false;
        }

        municionActual--;

        if (municionActual <= 0)
        {
            DetenerSonidoSMG();
        }

        return true;
    }

    public bool AgregarMunicion(int cantidad)
    {
        if (cantidad <= 0)
            return false;

        if (municionReserva >= reservaMaxima)
            return false;

        int municionAntes = municionReserva;

        municionReserva = Mathf.Clamp(
            municionReserva + cantidad,
            0,
            reservaMaxima
        );

        return municionReserva > municionAntes;
    }

    // =====================================================
    // RECARGA
    // =====================================================

    public void Recargar()
    {
        if (recargando)
            return;

        if (municionActual >= capacidadCargador)
            return;

        if (municionReserva <= 0)
            return;

        DetenerSonidoSMG();

        StartCoroutine(RecargarCoroutine());
    }

    protected IEnumerator RecargarCoroutine()
    {
        recargando = true;

        DetenerSonidoSMG();

        Debug.Log("Recargando " + nombreArma + "...");

        yield return new WaitForSeconds(tiempoRecarga);

        int espacioDisponible =
            capacidadCargador - municionActual;

        int cantidadRecargar = Mathf.Min(
            espacioDisponible,
            municionReserva
        );

        municionActual += cantidadRecargar;
        municionReserva -= cantidadRecargar;

        recargando = false;

        Debug.Log(
            "Recarga terminada: " +
            municionActual +
            " / " +
            municionReserva
        );
    }

    // =====================================================
    // PROPIEDADES
    // =====================================================

    public int MunicionActual => municionActual;

    public int MunicionReserva => municionReserva;

    public int CapacidadCargador => capacidadCargador;

    public int ReservaMaxima => reservaMaxima;

    public bool EstaRecargando => recargando;

    // =====================================================
    // LIMPIEZA
    // =====================================================

    protected virtual void OnDisable()
    {
        DetenerSonidoSMG();
    }
}
