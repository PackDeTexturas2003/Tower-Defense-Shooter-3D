using UnityEngine;

public class ZonaColocacionTorre : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer visualZona;

    [Header("Colores")]
    [SerializeField]
    private Color colorDisponible =
        new Color(0.2f, 1f, 0.2f, 0.45f);

    [SerializeField]
    private Color colorOcupada =
        new Color(1f, 0.2f, 0.2f, 0.45f);

    [SerializeField]
    private Color colorSeleccionada =
        new Color(1f, 1f, 0.2f, 0.55f);

    private Color colorOriginal;

    private Material materialVisual;

    private bool ocupada;

    private GameObject torreColocada;

    private void Awake()
    {
        if (visualZona == null)
        {
            visualZona =
                GetComponent<Renderer>();
        }

        if (visualZona != null)
        {
            materialVisual =
                visualZona.material;

            colorOriginal =
                materialVisual.color;
        }
    }

    // =====================================================
    // ESTADO DE LA ZONA
    // =====================================================

    public bool EstaOcupada()
    {
        return ocupada;
    }

    public bool PuedeColocar()
    {
        return !ocupada;
    }

    // =====================================================
    // REGISTRAR TORRE
    // =====================================================

    public void RegistrarTorre(
        GameObject torre)
    {
        if (torre == null)
            return;

        torreColocada = torre;

        ocupada = true;

        MostrarColorOcupada();

        Debug.Log(
            "Zona ocupada: " +
            gameObject.name
        );
    }

    // =====================================================
    // LIBERAR ZONA
    // =====================================================

    public void LiberarZona()
    {
        torreColocada = null;

        ocupada = false;

        MostrarColorDisponible();
    }

    // =====================================================
    // COLORES
    // =====================================================

    public void MostrarColorDisponible()
    {
        if (materialVisual == null)
            return;

        materialVisual.color =
            colorDisponible;
    }

    public void MostrarColorOcupada()
    {
        if (materialVisual == null)
            return;

        materialVisual.color =
            colorOcupada;
    }

    public void MostrarColorSeleccionada()
    {
        if (materialVisual == null)
            return;

        materialVisual.color =
            colorSeleccionada;
    }

    public void RestaurarColorOriginal()
    {
        if (materialVisual == null)
            return;

        materialVisual.color =
            colorOriginal;
    }

    // =====================================================
    // OBTENER TORRE
    // =====================================================

    public GameObject ObtenerTorre()
    {
        return torreColocada;
    }
}