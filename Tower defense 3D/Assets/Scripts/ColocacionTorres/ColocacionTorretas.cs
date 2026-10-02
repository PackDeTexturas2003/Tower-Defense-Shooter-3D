using UnityEngine;
using UnityEngine.InputSystem;

public class ColocacionTorretas : MonoBehaviour
{
    [Header("Torre")]
    [SerializeField] private GameObject prefabTorre;

    [Header("Configuración")]
    [SerializeField] private Camera camaraPlanificacion;
    [SerializeField] private LayerMask capaSuelo;

    private bool puedeColocar = true;

    private void Update()
    {
        if (!puedeColocar)
            return;

        if (Mouse.current == null)
            return;

        if (camaraPlanificacion == null)
            return;

        // Clic izquierdo del mouse
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ColocarTorre();
        }
    }

    private void ColocarTorre()
    {
        Vector2 posicionMouse = Mouse.current.position.ReadValue();

        Ray rayo = camaraPlanificacion.ScreenPointToRay(posicionMouse);

        if (Physics.Raycast(rayo, out RaycastHit impacto, 1000f, capaSuelo))
        {
            if (prefabTorre != null)
            {
                Instantiate(
                    prefabTorre,
                    impacto.point,
                    Quaternion.identity
                );
            }
        }
    }

    public void DetenerColocacion()
    {
        puedeColocar = false;
    }
}