
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

        CargarTeclaSalto();

        rb.useGravity = true;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CargarTeclaSalto();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[teclaSalto].wasPressedThisFrame)
        {
            IntentarSaltar();
        }
    }

    // =========================
    // CARGAR SALTO
    // =========================

    private void CargarTeclaSalto()
    {
        teclaSalto = ConfiguracionControles.ObtenerSalto();
    }

    // =========================
    // INTENTAR SALTAR
    // =========================

    private void IntentarSaltar()
    {
        if (Time.time < tiempoUltimoSalto + cooldownSalto)
            return;

        if (!estaEnSuelo)
            return;

        Saltar();
    }

    // =========================
    // REALIZAR SALTO
    // =========================

    private void Saltar()
    {
        tiempoUltimoSalto = Time.time;

        Vector3 velocidad = rb.linearVelocity;

        velocidad.y = 0f;

        rb.linearVelocity = velocidad;

        rb.AddForce(
            Vector3.up * fuerzaSalto,
            ForceMode.Impulse
        );

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

            float angulo = Vector3.Angle(
                contacto.normal,
                Vector3.up
            );

            if (angulo <= anguloMaximoSuelo)
            {
                estaEnSuelo = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        estaEnSuelo = false;
    }
}
