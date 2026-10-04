using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ColocacionTorretas : MonoBehaviour
{
    [Header("Torre")]
    [SerializeField] private GameObject prefabTorre;

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
            if (prefabTorre == null)
                return;

            Instantiate(
                prefabTorre,
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
}