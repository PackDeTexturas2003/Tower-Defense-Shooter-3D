
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float sensibilidad = 2.5f;

    [Header("Límite de visión vertical")]
    [Tooltip("Máximo de grados que puede mirar hacia arriba o abajo desde la posición inicial.")]
    [SerializeField] private float limiteVertical = 60f;

    private float rotacionVertical;
    private float rotacionInicialX;

    private void Start()
    {
        GuardarRotacionInicial();
    }

    private void Update()
    {
        RotarCamara();
    }

    private void GuardarRotacionInicial()
    {
        rotacionInicialX = transform.localEulerAngles.x;

        if (rotacionInicialX > 180f)
            rotacionInicialX -= 360f;

        rotacionVertical = 0f;
    }

    private void RotarCamara()
    {
        if (Mouse.current == null)
            return;

        Vector2 movimientoMouse = Mouse.current.delta.ReadValue();

        // Utiliza exactamente la misma sensibilidad que el giro horizontal.
        float sensibilidadActual =
            ConfiguracionControles.ObtenerSensibilidadMouse();

        float sentidoVertical =
            ConfiguracionControles.ObtenerInvertirEjeY() ? 1f : -1f;

        rotacionVertical += movimientoMouse.y *
                            sensibilidadActual *
                            sentidoVertical;

        rotacionVertical = Mathf.Clamp(
            rotacionVertical,
            -limiteVertical,
            limiteVertical
        );

        transform.localRotation = Quaternion.Euler(
            rotacionInicialX + rotacionVertical,
            0f,
            0f
        );
    }

    public void RestaurarRotacionInicial()
    {
        rotacionVertical = 0f;

        transform.localRotation = Quaternion.Euler(
            rotacionInicialX,
            0f,
            0f
        );
    }
}
