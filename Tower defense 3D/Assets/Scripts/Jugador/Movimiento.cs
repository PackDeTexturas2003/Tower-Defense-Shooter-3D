
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movimiento : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Sprint")]
    [SerializeField] private bool sprintActivado = true;
    [SerializeField] private float velocidadSprint = 8f;

    [Header("Cámara")]
    [SerializeField] private float sensibilidadMouse = 2.5f;

    private Rigidbody rb;
    private Esquive esquive;

    private Key teclaSprint;
    private Key teclaAdelante;
    private Key teclaIzquierda;
    private Key teclaAtras;
    private Key teclaDerecha;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        esquive = GetComponent<Esquive>();

        CargarConfiguracionControles();

        rb.freezeRotation = true;
        rb.useGravity = true;
    }

    private void Update()
    {
        CargarConfiguracionControles();
        Mirar();
    }

    private void FixedUpdate()
    {
        if (esquive != null && esquive.EstaHaciendoDash())
            return;

        Mover();
    }

    // =========================
    // CARGAR CONFIGURACIÓN
    // =========================

    private void CargarConfiguracionControles()
    {
        teclaSprint = ConfiguracionControles.ObtenerSprint();

        teclaAdelante = ConfiguracionControles.ObtenerAdelante();
        teclaIzquierda = ConfiguracionControles.ObtenerIzquierda();
        teclaAtras = ConfiguracionControles.ObtenerAtras();
        teclaDerecha = ConfiguracionControles.ObtenerDerecha();
    }

    // =========================
    // MOVIMIENTO
    // =========================

    private void Mover()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current[teclaIzquierda].isPressed)
            horizontal = -1f;

        if (Keyboard.current[teclaDerecha].isPressed)
            horizontal = 1f;

        if (Keyboard.current[teclaAdelante].isPressed)
            vertical = 1f;

        if (Keyboard.current[teclaAtras].isPressed)
            vertical = -1f;

        Vector3 direccion =
            transform.right * horizontal +
            transform.forward * vertical;

        direccion.y = 0f;

        if (direccion.sqrMagnitude > 1f)
            direccion.Normalize();

        float velocidadActual = velocidad;

        if (sprintActivado &&
            Keyboard.current[teclaSprint].isPressed)
        {
            velocidadActual = velocidadSprint;
        }

        Vector3 nuevaVelocidad =
            direccion * velocidadActual;

        nuevaVelocidad.y = rb.linearVelocity.y;

        rb.linearVelocity = nuevaVelocidad;
    }

    // =========================
    // CÁMARA
    // =========================

    private void Mirar()
    {
        if (Mouse.current == null)
            return;

        Vector2 movimientoMouse =
            Mouse.current.delta.ReadValue();

        float mouseX =
            movimientoMouse.x * sensibilidadMouse;

        transform.Rotate(
            Vector3.up * mouseX
        );
    }
}
