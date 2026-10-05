using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ColocacionTorretas : MonoBehaviour
{
    [Header("Torres")]
    [SerializeField] private GameObject[] prefabsTorres;

    [SerializeField] private int indiceTorreSeleccionada = 0;

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

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        ColocarTorre();
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
    }
}