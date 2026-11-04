
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeleccionArmas : MonoBehaviour
{
    [System.Serializable]
    public class OpcionArma
    {
        public string nombre;
        public int precio;
        public Button boton;

        [HideInInspector]
        public Weapon arma;
    }

    [Header("Armas disponibles")]
    [SerializeField] private OpcionArma[] opcionesArmas;

    [Header("Referencias")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GamePhaseManager gamePhaseManager;
    [SerializeField] private GameObject panelSeleccionArmas;

    [Header("Configuración")]
    [SerializeField] private int maximoArmas = 2;

    [Header("Dinero")]
    [SerializeField] private DineroJugador dineroJugador;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text textoDineroDisponible;
    [SerializeField] private TMP_Text textoCostoSeleccion;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Confirmación")]
    [SerializeField] private Button botonConfirmar;

    private readonly List<OpcionArma> armasSeleccionadas =
        new List<OpcionArma>();

    private int costoSeleccion;
    private bool seleccionConfirmada;

    public bool SeleccionConfirmada => seleccionConfirmada;

    public List<Weapon> ObtenerArmasSeleccionadas()
    {
        List<Weapon> resultado = new List<Weapon>();

        foreach (OpcionArma opcion in armasSeleccionadas)
        {
            if (opcion != null && opcion.arma != null)
                resultado.Add(opcion.arma);
        }

        return resultado;
    }

    private void Awake()
    {
        BuscarArmas();

        if (opcionesArmas != null)
        {
            foreach (OpcionArma opcion in opcionesArmas)
            {
                if (opcion == null || opcion.boton == null)
                    continue;

                OpcionArma opcionActual = opcion;

                opcion.boton.onClick.AddListener(
                    () => AlternarSeleccion(opcionActual)
                );
            }
        }

        if (botonConfirmar != null)
            botonConfirmar.onClick.AddListener(ConfirmarSeleccion);
    }

    private void Start()
    {
        if (panelSeleccionArmas == null)
            panelSeleccionArmas = gameObject;

        ActualizarInterfaz();

        MostrarMensaje("");
    }

    private void BuscarArmas()
    {
        if (weaponHolder == null)
        {
            Debug.LogError(
                "SeleccionArmas: asigna el objeto WeaponHolder."
            );
            return;
        }

        Weapon[] armasEncontradas =
            weaponHolder.GetComponents<Weapon>();

        if (opcionesArmas == null)
            return;

        foreach (OpcionArma opcion in opcionesArmas)
        {
            if (opcion == null)
                continue;

            opcion.arma = null;

            foreach (Weapon arma in armasEncontradas)
            {
                if (arma == null)
                    continue;

                string tipo = arma.GetType().Name;

                bool coincide =
                    (opcion.nombre == "Pistola" && tipo == "Pistol") ||
                    (opcion.nombre == "Escopeta" && tipo == "Shotgun") ||
                    (opcion.nombre == "SMG" && tipo == "SMG") ||
                    (opcion.nombre == "Rifle" && tipo == "BurstRifle") ||
                    (opcion.nombre == "Sniper" && tipo == "BoltRifle") ||
                    (opcion.nombre == "RPG" && tipo == "RPG");

                if (coincide)
                {
                    opcion.arma = arma;

                    Debug.Log(
                        "SeleccionArmas: " + opcion.nombre +
                        " conectada con " + tipo
                    );

                    break;
                }
            }

            if (opcion.arma == null)
            {
                Debug.LogError(
                    "SeleccionArmas: no se encontró el arma " +
                    opcion.nombre + "."
                );
            }
        }
    }

    private bool EstaDesbloqueada(OpcionArma opcion)
    {
        if (opcion == null)
            return false;

        // Si no existe SaveManager, solo se considera gratis
        // el arma cuyo precio sea cero.
        if (SaveManager.Instance == null)
            return opcion.precio <= 0;

        return SaveManager.Instance.EstaDesbloqueada(opcion.nombre);
    }

    private int ObtenerPrecio(OpcionArma opcion)
    {
        if (EstaDesbloqueada(opcion))
            return 0;

        return Mathf.Max(0, opcion.precio);
    }

    private void AlternarSeleccion(OpcionArma opcion)
    {
        if (seleccionConfirmada || opcion == null)
            return;

        if (armasSeleccionadas.Contains(opcion))
        {
            armasSeleccionadas.Remove(opcion);
        }
        else
        {
            if (armasSeleccionadas.Count >= maximoArmas)
            {
                MostrarMensaje(
                    "Solo puedes seleccionar " +
                    maximoArmas + " armas."
                );
                return;
            }

            if (opcion.arma == null)
            {
                MostrarMensaje(
                    "No se encontró el componente de " +
                    opcion.nombre + "."
                );
                return;
            }

            int precioArma = ObtenerPrecio(opcion);

            int dineroDisponible = dineroJugador != null
                ? dineroJugador.ObtenerDinero()
                : 0;

            if (costoSeleccion + precioArma > dineroDisponible)
            {
                MostrarMensaje("No tienes suficiente dinero.");
                return;
            }

            armasSeleccionadas.Add(opcion);
        }

        CalcularCosto();
        ActualizarInterfaz();

        MostrarMensaje("");
    }

    private void CalcularCosto()
    {
        costoSeleccion = 0;

        foreach (OpcionArma opcion in armasSeleccionadas)
        {
            if (opcion != null)
                costoSeleccion += ObtenerPrecio(opcion);
        }
    }

    private void ActualizarInterfaz()
    {
        int dineroDisponible = dineroJugador != null
            ? dineroJugador.ObtenerDinero()
            : 0;

        if (textoDineroDisponible != null)
        {
            textoDineroDisponible.text =
                "Dinero disponible: $" + dineroDisponible;
        }

        if (textoCostoSeleccion != null)
        {
            textoCostoSeleccion.text =
                "Costo de armas: $" + costoSeleccion;
        }

        if (opcionesArmas != null)
        {
            foreach (OpcionArma opcion in opcionesArmas)
            {
                if (opcion == null || opcion.boton == null)
                    continue;

                TMP_Text textoBoton =
                    opcion.boton.GetComponentInChildren<TMP_Text>();

                bool seleccionada =
                    armasSeleccionadas.Contains(opcion);

                bool desbloqueada =
                    EstaDesbloqueada(opcion);

                if (textoBoton != null)
                {
                    string estado = desbloqueada
                        ? " [COMPRADA]"
                        : "";

                    string seleccion = seleccionada
                        ? "[SELECCIONADA] "
                        : "";

                    string precio = desbloqueada
                        ? "Gratis"
                        : "$" + Mathf.Max(0, opcion.precio);

                    textoBoton.text =
                        seleccion +
                        opcion.nombre +
                        estado +
                        " — " +
                        precio;
                }

                opcion.boton.interactable = !seleccionConfirmada;
            }
        }

        if (botonConfirmar != null)
        {
            botonConfirmar.interactable = !seleccionConfirmada;
        }
    }

    private void ConfirmarSeleccion()
    {
        if (seleccionConfirmada)
            return;

        if (armasSeleccionadas.Count == 0)
        {
            MostrarMensaje(
                "Selecciona al menos un arma para continuar."
            );
            return;
        }

        if (dineroJugador == null)
        {
            MostrarMensaje(
                "Falta asignar DineroJugador en el Inspector."
            );
            return;
        }

        if (weaponManager == null)
        {
            MostrarMensaje(
                "Falta asignar WeaponManager en el Inspector."
            );
            return;
        }

        if (gamePhaseManager == null)
        {
            MostrarMensaje(
                "Falta asignar GamePhaseManager en el Inspector."
            );
            return;
        }

        if (SaveManager.Instance == null)
        {
            MostrarMensaje(
                "No se encontró el SaveManager."
            );
            return;
        }

        CalcularCosto();

        if (!dineroJugador.TieneDinero(costoSeleccion))
        {
            MostrarMensaje("No tienes suficiente dinero.");
            return;
        }

        List<Weapon> seleccionadas = ObtenerArmasSeleccionadas();

        if (seleccionadas.Count == 0)
        {
            MostrarMensaje(
                "Selecciona al menos un arma para continuar."
            );
            return;
        }

        // Configura las armas seleccionadas.
        weaponManager.ConfigurarArmasSeleccionadas(seleccionadas);

        // Cobra solamente las armas que todavía no estaban compradas.
        if (!dineroJugador.GastarDinero(costoSeleccion))
        {
            MostrarMensaje("No se pudo realizar el pago.");
            return;
        }

        // Guarda las armas seleccionadas como desbloqueadas.
        foreach (OpcionArma opcion in armasSeleccionadas)
        {
            if (opcion != null)
            {
                SaveManager.Instance.DesbloquearArma(opcion.nombre);
            }
        }

        // Guarda los datos de la partida.
        SaveManager.Instance.GuardarTodo();

        seleccionConfirmada = true;
        ActualizarInterfaz();

        MostrarMensaje(
            "Armas confirmadas. Dinero restante: $" +
            dineroJugador.ObtenerDinero()
        );

        Debug.Log(
            "Selección confirmada. Armas: " +
            seleccionadas.Count +
            ". Costo: $" + costoSeleccion +
            ". Dinero restante: $" +
            dineroJugador.ObtenerDinero()
        );

        if (panelSeleccionArmas != null)
            panelSeleccionArmas.SetActive(false);

        gamePhaseManager.ConfirmarSeleccionArmas();
    }

    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje != null)
            textoMensaje.text = mensaje;
    }
}
