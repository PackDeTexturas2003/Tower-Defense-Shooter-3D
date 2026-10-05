using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColocacionTorretas : MonoBehaviour
{
    [Header("Torres")]
    [SerializeField] private GameObject[] prefabsTorres;

    [SerializeField] private int indiceTorreSeleccionada = 0;

    [Header("Imágenes de selección")]
    [SerializeField] private Image[] imagenesTorres;

    [Header("Configuración")]
    [SerializeField] private Camera camaraPlanificacion;
    [SerializeField] private LayerMask capaSuelo;

    private bool puedeColocar;

    private void Update()
    {
        if (!puedeColocar)
            return;

        if (Mouse.current == null)
            return;

        if (camaraPlanificacion == null)
            return;

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

    private bool SeleccionarTorreDesdeUI()
    {
        if (imagenesTorres == null ||
            imagenesTorres.Length == 0)
        {
            return false;
        }

        Vector2 posicionMouse = Mouse.current.position.ReadValue();

        PointerEventData evento =
            new PointerEventData(EventSystem.current);

        evento.position = posicionMouse;

        System.Collections.Generic.List<RaycastResult> resultados =
            new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(evento, resultados);

        for (int i = 0; i < resultados.Count; i++)
        {
            GameObject objetoImpactado = resultados[i].gameObject;

            for (int j = 0; j < imagenesTorres.Length; j++)
            {
                if (imagenesTorres[j] == null)
                    continue;

                if (objetoImpactado == imagenesTorres[j].gameObject ||
                    objetoImpactado.transform.IsChildOf(imagenesTorres[j].transform))
                {
                    SeleccionarTorre(j);
                    return true;
                }
            }
        }

        return false;
    }

    private void ColocarTorre()
    {
        Vector2 posicionMouse = Mouse.current.position.ReadValue();

        Ray rayo = camaraPlanificacion.ScreenPointToRay(posicionMouse);

        if (Physics.Raycast(rayo, out RaycastHit impacto, 1000f, capaSuelo))
        {
            if (prefabsTorres == null ||
                prefabsTorres.Length == 0)
            {
                return;
            }

            if (indiceTorreSeleccionada < 0 ||
                indiceTorreSeleccionada >= prefabsTorres.Length)
            {
                return;
            }

            GameObject prefabSeleccionado =
                prefabsTorres[indiceTorreSeleccionada];

            if (prefabSeleccionado == null)
                return;

            Instantiate(
                prefabSeleccionado,
                impacto.point,
                Quaternion.identity
            );
        }
    }

    public void IniciarColocacion()
    {
        puedeColocar = true;
    }

    public void DetenerColocacion()
    {
        puedeColocar = false;
    }

    public void SeleccionarTorre(int indice)
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

        indiceTorreSeleccionada = indice;

        Debug.Log(
            "Torre seleccionada: " +
            indiceTorreSeleccionada
        );
    }
}