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
    protected AmmoType tipoMunicion =
        AmmoType.Pistola;

    public AmmoType TipoMunicion => tipoMunicion;

    [Header("Modo de Disparo")]
    [SerializeField]
    protected FireMode fireMode =
        FireMode.SemiAuto;

    public FireMode FireMode => fireMode;

    [Header("Disparo")]
    public float dańo = 10f;
    public float tiempoEntreDisparos = 1f;
    public float distancia = 100f;

    [Header("Retroceso del jugador")]
    [SerializeField] protected float fuerzaRetrocesoJugador = 1f;

    [Header("Empuje al enemigo")]
    [SerializeField] protected float fuerzaEmpujeEnemigo = 2f;

    [Header("Munición")]
    [SerializeField] protected int capacidadCargador = 12;
    [SerializeField] protected int municionInicial = 12;
    [SerializeField] protected int reservaMaxima = 60;
    [SerializeField] protected int municionReservaInicial = 60;

    [Header("Recarga")]
    [SerializeField] protected float tiempoRecarga = 1.5f;

    protected Camera camara;
    protected float siguienteDisparo;
    protected int municionActual;
    protected int municionReserva;
    protected bool recargando;

    protected virtual void Awake()
    {
        camara = Camera.main;

        municionActual =
            Mathf.Clamp(
                municionInicial,
                0,
                capacidadCargador
            );

        municionReserva =
            Mathf.Clamp(
                municionReservaInicial,
                0,
                reservaMaxima
            );
    }

    public virtual void Disparar()
    {
    }

    public bool TieneMunicion()
    {
        return municionActual > 0;
    }

    protected bool ConsumirMunicion()
    {
        if (municionActual <= 0)
            return false;

        municionActual--;

        return true;
    }

    public bool AgregarMunicion(
        int cantidad)
    {
        if (cantidad <= 0)
            return false;

        if (municionReserva >= reservaMaxima)
            return false;

        int municionAntes =
            municionReserva;

        municionReserva =
            Mathf.Clamp(
                municionReserva + cantidad,
                0,
                reservaMaxima
            );

        return municionReserva >
               municionAntes;
    }

    public void Recargar()
    {
        if (recargando)
            return;

        if (municionActual >= capacidadCargador)
            return;

        if (municionReserva <= 0)
            return;

        StartCoroutine(
            RecargarCoroutine()
        );
    }

    protected IEnumerator RecargarCoroutine()
    {
        recargando = true;

        Debug.Log(
            "Recargando " +
            nombreArma +
            "..."
        );

        yield return new WaitForSeconds(
            tiempoRecarga
        );

        int espacioDisponible =
            capacidadCargador -
            municionActual;

        int cantidadRecargar =
            Mathf.Min(
                espacioDisponible,
                municionReserva
            );

        municionActual +=
            cantidadRecargar;

        municionReserva -=
            cantidadRecargar;

        recargando = false;

        Debug.Log(
            "Recarga terminada: " +
            municionActual +
            " / " +
            municionReserva
        );
    }

    public int MunicionActual =>
        municionActual;

    public int MunicionReserva =>
        municionReserva;

    public int CapacidadCargador =>
        capacidadCargador;

    public int ReservaMaxima =>
        reservaMaxima;

    public bool EstaRecargando =>
        recargando;
}