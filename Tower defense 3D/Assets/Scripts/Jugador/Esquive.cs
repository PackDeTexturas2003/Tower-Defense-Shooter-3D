using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Esquive : MonoBehaviour
{
    [Header("Dash")]
    [Tooltip("Velocidad máxima del dash.")]
    [SerializeField] private float velocidadDash = 20f;

    [Tooltip("Velocidad con la que el dash alcanza su velocidad máxima.")]
    [SerializeField] private float aceleracionDash = 100f;

    [Tooltip("Duración total del dash.")]
    [SerializeField] private float duracionDash = 0.2f;

    [Tooltip("Tiempo de espera antes de volver a usar el dash.")]
    [SerializeField] private float cooldownDash = 1f;

    [Header("Opciones")]
    [Tooltip("Permite realizar el dash mientras estás en el aire.")]
    [SerializeField] private bool permitirDashAereo = true;

    [Tooltip("Si no pulsas ninguna dirección, el dash irá hacia delante.")]
    [SerializeField] private bool dashHaciaDelanteSinMovimiento = true;

    private Rigidbody rb;

    private bool haciendoDash;

    private float tiempoDash;
    private float tiempoUltimoDash = -Mathf.Infinity;

    private Vector3 direccionDash;

    private Key teclaDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        CargarTeclaDash();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[teclaDash].wasPressedThisFrame)
        {
            IntentarDash();
        }
    }

    private void FixedUpdate()
    {
        if (!haciendoDash)
            return;

        EjecutarDash();
    }

    // =========================
    // GUARDAR / CARGAR DASH
    // =========================

    private void CargarTeclaDash()
    {
        teclaDash = (Key)PlayerPrefs.GetInt(
            "TeclaDash",
            (int)Key.E
        );
    }

    // =========================
    // INTENTAR DASH
    // =========================

    private void IntentarDash()
    {
        // No permitir otro dash mientras ya estamos haciendo uno.
        if (haciendoDash)
            return;

        // Comprobar cooldown.
        if (Time.time < tiempoUltimoDash + cooldownDash)
            return;

        // Obtener dirección del movimiento.
        direccionDash = ObtenerDireccion();

        // Si no se está pulsando WASD,
        // utilizar la dirección hacia delante.
        if (direccionDash == Vector3.zero)
        {
            if (!dashHaciaDelanteSinMovimiento)
                return;

            direccionDash = transform.forward;
        }

        // Evitar movimiento vertical.
        direccionDash.y = 0f;

        // Asegurarnos de tener una dirección válida.
        if (direccionDash.sqrMagnitude < 0.01f)
            return;

        direccionDash.Normalize();

        // Activar dash.
        haciendoDash = true;

        tiempoDash = 0f;
        tiempoUltimoDash = Time.time;

        // Reiniciar únicamente la velocidad horizontal.
        Vector3 velocidad = rb.linearVelocity;

        velocidad.x = 0f;
        velocidad.z = 0f;

        rb.linearVelocity = velocidad;
    }

    // =========================
    // EJECUTAR DASH
    // =========================

    private void EjecutarDash()
    {
        tiempoDash += Time.fixedDeltaTime;

        // Velocidad progresiva.
        float velocidadActual = Mathf.MoveTowards(
            0f,
            velocidadDash,
            aceleracionDash * Time.fixedDeltaTime
        );

        // Mantener la velocidad vertical.
        float velocidadVertical = rb.linearVelocity.y;

        // Aplicar velocidad del dash.
        Vector3 nuevaVelocidad =
            direccionDash * velocidadActual;

        nuevaVelocidad.y = velocidadVertical;

        rb.linearVelocity = nuevaVelocidad;

        // Comprobar si terminó.
        if (tiempoDash >= duracionDash)
        {
            TerminarDash();
        }
    }

    // =========================
    // TERMINAR DASH
    // =========================

    private void TerminarDash()
    {
        haciendoDash = false;
    }

    // =========================
    // OBTENER DIRECCIÓN
    // =========================

    private Vector3 ObtenerDireccion()
    {
        if (Keyboard.current == null)
            return Vector3.zero;

        float horizontal = 0f;
        float vertical = 0f;

        // =========================
        // WASD FIJO
        // =========================

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical = 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical = -1f;

        Vector3 direccion =
            transform.right * horizontal +
            transform.forward * vertical;

        direccion.y = 0f;

        return direccion.normalized;
    }

    // =========================
    // ESTADO DEL DASH
    // =========================

    public bool EstaHaciendoDash()
    {
        return haciendoDash;
    }

    // =========================
    // GIZMOS
    // =========================

    private void OnDrawGizmosSelected()
    {
        if (!haciendoDash)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            transform.position,
            direccionDash * 3f
        );
    }
}