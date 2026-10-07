using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColocacionTorretas : MonoBehaviour
{
    [Header("Torres")]
    [SerializeField] private GameObject[] prefabsTorres;

    [SerializeField] private int[] preciosTorres;

    [SerializeField] private int indiceTorreSeleccionada = 0;

    [Header("Imágenes de selección")]
    [SerializeField] private Image[] imagenesTorres;

    [Header("Dinero")]
    [SerializeField] private DineroJugador dineroJugador;

    [Header("Configuración")]
    [SerializeField] private Camera camaraPlanificacion;

    [SerializeField] private LayerMask capaSuelo;

    [Header("Zonas de colocación")]
    [SerializeField] private ZonaColocacionTorre[] zonasColocacion;

    [Header("Visual")]
    [SerializeField] private bool mostrarColores = true;

    private bool puedeColocar;

    private ZonaColocacionTorre zonaSeleccionadaActual;

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!puedeColocar)
            return;

        if (Mouse.current == null)
            return;

        if (camaraPlanificacion == null)
            return;

        ActualizarZonaBajoMouse();

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (SeleccionarTorreDesdeUI())
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        ColocarTorre();
    }

    // =====================================================
    // DETECTAR ZONA BAJO EL MOUSE
    // =====================================================

    private void ActualizarZonaBajoMouse()
    {
        ZonaColocacionTorre nuevaZona =
            BuscarZonaDesdeMouse();

        if (nuevaZona ==
            zonaSeleccionadaActual)
        {
            if (nuevaZona != null)
            {
                ActualizarColorZona(
                    nuevaZona
                );
            }

            return;
        }

        // Restaurar la zona anterior
        if (zonaSeleccionadaActual != null)
        {
            ActualizarColorZonaSinSeleccion(
                zonaSeleccionadaActual
            );
        }

        zonaSeleccionadaActual =
            nuevaZona;

        // Colorear la nueva zona
        if (zonaSeleccionadaActual != null)
        {
            ActualizarColorZona(
                zonaSeleccionadaActual
            );
        }
    }

    // =====================================================
    // BUSCAR ZONA DESDE EL MOUSE
    // =====================================================

    private ZonaColocacionTorre BuscarZonaDesdeMouse()
    {
        if (zonasColocacion == null ||
            zonasColocacion.Length == 0)
        {
            return null;
        }

        Vector2 posicionMouse =
            Mouse.current.position.ReadValue();

        Ray rayo =
            camaraPlanificacion.ScreenPointToRay(
                posicionMouse
            );

        ZonaColocacionTorre zonaEncontrada = null;

        float distanciaMasCercana =
            float.MaxValue;

        for (int i = 0;
             i < zonasColocacion.Length;
             i++)
        {
            if (zonasColocacion[i] == null)
                continue;

            Collider colliderZona =
                zonasColocacion[i].GetComponent<Collider>();

            if (colliderZona == null)
                continue;

            if (colliderZona.Raycast(
                rayo,
                out RaycastHit hit,
                1000f))
            {
                if (hit.distance <
                    distanciaMasCercana)
                {
                    distanciaMasCercana =
                        hit.distance;

                    zonaEncontrada =
                        zonasColocacion[i];
                }
            }
        }

        return zonaEncontrada;
    }

    // =====================================================
    // ACTUALIZAR COLOR
    // =====================================================

    private void ActualizarColorZona(
        ZonaColocacionTorre zona)
    {
        if (!mostrarColores)
            return;

        if (zona == null)
            return;

        if (zona.EstaOcupada())
        {
            zona.MostrarColorOcupada();

            return;
        }

        zona.MostrarColorSeleccionada();
    }

    private void ActualizarColorZonaSinSeleccion(
        ZonaColocacionTorre zona)
    {
        if (!mostrarColores)
            return;

        if (zona == null)
            return;

        if (zona.EstaOcupada())
        {
            zona.MostrarColorOcupada();
        }
        else
        {
            zona.MostrarColorDisponible();
        }
    }

    // =====================================================
    // SELECCIONAR TORRE DESDE LA UI
    // =====================================================

    private bool SeleccionarTorreDesdeUI()
    {
        if (imagenesTorres == null ||
            imagenesTorres.Length == 0)
        {
            return false;
        }

        if (EventSystem.current == null)
            return false;

        Vector2 posicionMouse =
            Mouse.current.position.ReadValue();

        PointerEventData evento =
            new PointerEventData(
                EventSystem.current
            );

        evento.position =
            posicionMouse;

        System.Collections.Generic.List<RaycastResult>
            resultados =
            new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(
            evento,
            resultados
        );

        for (int i = 0;
             i < resultados.Count;
             i++)
        {
            GameObject objetoImpactado =
                resultados[i].gameObject;

            for (int j = 0;
                 j < imagenesTorres.Length;
                 j++)
            {
                if (imagenesTorres[j] == null)
                    continue;

                if (
                    objetoImpactado ==
                    imagenesTorres[j].gameObject
                    ||
                    objetoImpactado.transform.IsChildOf(
                        imagenesTorres[j].transform
                    )
                )
                {
                    SeleccionarTorre(j);

                    return true;
                }
            }
        }

        return false;
    }

    // =====================================================
    // COLOCAR TORRE
    // =====================================================

    private void ColocarTorre()
    {
        if (zonaSeleccionadaActual == null)
        {
            Debug.Log(
                "No puedes colocar una torre aquí. " +
                "Selecciona una zona de colocación."
            );

            return;
        }

        // =================================================
        // COMPROBAR SI ESTÁ OCUPADA
        // =================================================

        if (!zonaSeleccionadaActual.PuedeColocar())
        {
            Debug.Log(
                "Esta zona ya tiene una torre."
            );

            return;
        }

        // =================================================
        // COMPROBAR TORRES
        // =================================================

        if (prefabsTorres == null ||
            prefabsTorres.Length == 0)
        {
            return;
        }

        if (
            indiceTorreSeleccionada < 0 ||
            indiceTorreSeleccionada >=
            prefabsTorres.Length
        )
        {
            return;
        }

        GameObject prefabSeleccionado =
            prefabsTorres[
                indiceTorreSeleccionada
            ];

        if (prefabSeleccionado == null)
            return;

        // =================================================
        // PRECIO
        // =================================================

        int precio =
            ObtenerPrecioTorre(
                indiceTorreSeleccionada
            );

        if (dineroJugador == null)
        {
            Debug.LogWarning(
                "ColocacionTorretas: " +
                "No se asignó DineroJugador."
            );

            return;
        }

        if (!dineroJugador.TieneDinero(
            precio
        ))
        {
            Debug.Log(
                "No tienes suficiente dinero " +
                "para comprar esta torre. " +
                "Precio: " +
                precio +
                " | Dinero actual: " +
                dineroJugador.ObtenerDinero()
            );

            return;
        }

        // =================================================
        // GASTAR DINERO
        // =================================================

        if (!dineroJugador.GastarDinero(
            precio
        ))
        {
            return;
        }

        // =================================================
        // POSICIÓN
        // =================================================

        Vector3 posicionTorre =
            zonaSeleccionadaActual.transform.position;

        GameObject torre =
            Instantiate(
                prefabSeleccionado,
                posicionTorre,
                Quaternion.identity
            );

        // =================================================
        // REGISTRAR ZONA
        // =================================================

        zonaSeleccionadaActual.RegistrarTorre(
            torre
        );

        Debug.Log(
            "Torre colocada en zona: " +
            zonaSeleccionadaActual.gameObject.name +
            " | Precio: " +
            precio
        );
    }

    // =====================================================
    // PRECIO
    // =====================================================

    private int ObtenerPrecioTorre(
        int indice)
    {
        if (preciosTorres == null ||
            indice < 0 ||
            indice >= preciosTorres.Length)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            preciosTorres[indice]
        );
    }

    // =====================================================
    // INICIAR COLOCACIÓN
    // =====================================================

    public void IniciarColocacion()
    {
        puedeColocar = true;

        PrepararVisualesZonas();

        Debug.Log(
            "Sistema de colocación iniciado."
        );
    }

    // =====================================================
    // DETENER COLOCACIÓN
    // =====================================================

    public void DetenerColocacion()
    {
        puedeColocar = false;

        zonaSeleccionadaActual = null;

        RestaurarVisualesZonas();

        Debug.Log(
            "Sistema de colocación detenido."
        );
    }

    // =====================================================
    // PREPARAR VISUALES
    // =====================================================

    private void PrepararVisualesZonas()
    {
        if (!mostrarColores)
            return;

        if (zonasColocacion == null)
            return;

        for (int i = 0;
             i < zonasColocacion.Length;
             i++)
        {
            if (zonasColocacion[i] == null)
                continue;

            if (zonasColocacion[i].EstaOcupada())
            {
                zonasColocacion[i]
                    .MostrarColorOcupada();
            }
            else
            {
                zonasColocacion[i]
                    .MostrarColorDisponible();
            }
        }
    }

    // =====================================================
    // RESTAURAR VISUALES
    // =====================================================

    private void RestaurarVisualesZonas()
    {
        if (zonasColocacion == null)
            return;

        for (int i = 0;
             i < zonasColocacion.Length;
             i++)
        {
            if (zonasColocacion[i] == null)
                continue;

            zonasColocacion[i]
                .RestaurarColorOriginal();
        }
    }

    // =====================================================
    // SELECCIONAR TORRE
    // =====================================================

    public void SeleccionarTorre(
        int indice)
    {
        if (prefabsTorres == null ||
            prefabsTorres.Length == 0)
        {
            return;
        }

        if (indice < 0 ||
            indice >= prefabsTorres.Length)
        {
            return;
        }

        indiceTorreSeleccionada =
            indice;

        Debug.Log(
            "Torre seleccionada: " +
            indiceTorreSeleccionada
        );
    }
}