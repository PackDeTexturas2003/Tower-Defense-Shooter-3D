using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float sensibilidad = 0.1f;

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

        // Acumulamos poco a poco el movimiento vertical
        rotacionVertical -= movimientoMouse.y * sensibilidad;

        // Limitamos únicamente cuando realmente alcanzamos el límite
        rotacionVertical = Mathf.Clamp(
            rotacionVertical,
            -limiteVertical,
            limiteVertical
        );

        // Aplicamos la rotación inicial + el movimiento acumulado
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