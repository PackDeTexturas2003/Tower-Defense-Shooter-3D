using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Salto : MonoBehaviour
{
    [Header("Salto")]
    [Tooltip("Fuerza utilizada para realizar el salto.")]
    [SerializeField] private float fuerzaSalto = 7f;

    [Tooltip("Tiempo mínimo entre saltos.")]
    [SerializeField] private float cooldownSalto = 0.2f;

    [Header("Detección del suelo")]
    [Tooltip("Ángulo máximo que se considera suelo.")]
    [Range(0f, 90f)]
    [SerializeField] private float anguloMaximoSuelo = 60f;

    private Rigidbody rb;

    private bool estaEnSuelo;

    private float tiempoUltimoSalto = -Mathf.Infinity;

    private Key teclaSalto;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Cargar la tecla de salto guardada.
        CargarTeclaSalto();

        // Asegurar que el Rigidbody utilice gravedad.
        rb.useGravity = true;

        // Evitar que el personaje rote por las físicas.
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Detectar la tecla de salto.
        if (Keyboard.current[teclaSalto].wasPressedThisFrame)
        {
            IntentarSaltar();
        }
    }

    // =========================
    // GUARDAR / CARGAR SALTO
    // =========================

    private void CargarTeclaSalto()
    {
        teclaSalto = (Key)PlayerPrefs.GetInt(
            "TeclaSalto",
            (int)Key.Space
        );
    }

    // =========================
    // INTENTAR SALTAR
    // =========================

    private void IntentarSaltar()
    {
        // Comprobar cooldown.
        if (Time.time < tiempoUltimoSalto + cooldownSalto)
            return;

        // Comprobar si estamos en el suelo.
        if (!estaEnSuelo)
            return;

        Saltar();
    }

    // =========================
    // REALIZAR SALTO
    // =========================

    private void Saltar()
    {
        // Registrar el momento del salto.
        tiempoUltimoSalto = Time.time;

        // Obtener la velocidad actual.
        Vector3 velocidad = rb.linearVelocity;

        // Eliminar la velocidad vertical.
        velocidad.y = 0f;

        rb.linearVelocity = velocidad;

        // Aplicar el salto.
        rb.AddForce(
            Vector3.up * fuerzaSalto,
            ForceMode.Impulse
        );

        // Ya no estamos en el suelo.
        estaEnSuelo = false;
    }

    // =========================
    // DETECCIÓN DEL SUELO
    // =========================

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contacto = collision.GetContact(i);

            // Vector que indica hacia arriba desde la superficie.
            float angulo = Vector3.Angle(
                contacto.normal,
                Vector3.up
            );

            // Si la superficie es suficientemente horizontal,
            // la consideramos suelo.
            if (angulo <= anguloMaximoSuelo)
            {
                estaEnSuelo = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Esperamos a que OnCollisionStay vuelva a detectar
        // una superficie antes de permitir otro salto.
        estaEnSuelo = false;
    }
}