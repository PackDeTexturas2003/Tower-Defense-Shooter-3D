using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform camara;

    [Header("Detección")]
    [SerializeField] private LayerMask capasObstaculos;

    [SerializeField] private float distanciaDeteccion = 1.2f;
    [SerializeField] private float radioDeteccion = 0.25f;

    [Header("Desplazamiento de detección")]
    [Tooltip("Mueve el origen de detección hacia la derecha, donde está el arma.")]
    [SerializeField] private float desplazamientoDerecha = 0.35f;

    [SerializeField] private float desplazamientoAbajo = 0.1f;

    [Header("Movimiento al acercarse a pared")]
    [SerializeField] private float retroceso = 0.5f;
    [SerializeField] private float bajarArma = 0.1f;

    [Header("Rotación al acercarse a pared")]
    [SerializeField] private float anguloSubir = 55f;

    [Header("Suavidad")]
    [SerializeField] private float velocidadPosicion = 15f;
    [SerializeField] private float velocidadRotacion = 15f;

    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;

    void Start()
    {
        posicionOriginal = transform.localPosition;
        rotacionOriginal = transform.localRotation;

        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (camara == null)
            return;

        ComprobarPared();
    }

    void ComprobarPared()
    {
        Vector3 origenDeteccion =
            camara.position +
            camara.right * desplazamientoDerecha -
            camara.up * desplazamientoAbajo;

        bool hayPared = Physics.SphereCast(
            origenDeteccion,
            radioDeteccion,
            camara.forward,
            out RaycastHit hit,
            distanciaDeteccion,
            capasObstaculos,
            QueryTriggerInteraction.Ignore
        );

        Vector3 posicionObjetivo = posicionOriginal;
        Quaternion rotacionObjetivo = rotacionOriginal;

        if (hayPared)
        {
            float cercania =
                1f -
                Mathf.Clamp01(
                    hit.distance /
                    distanciaDeteccion
                );

            posicionObjetivo =
                posicionOriginal +
                Vector3.back *
                retroceso *
                cercania +
                Vector3.down *
                bajarArma *
                cercania;

            rotacionObjetivo =
                rotacionOriginal *
                Quaternion.Euler(
                    -anguloSubir * cercania,
                    0f,
                    0f
                );
        }

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                posicionObjetivo,
                velocidadPosicion *
                Time.deltaTime
            );

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                rotacionObjetivo,
                velocidadRotacion *
                Time.deltaTime
            );
    }

    void OnDrawGizmosSelected()
    {
        if (camara == null)
            return;

        Vector3 origen =
            camara.position +
            camara.right * desplazamientoDerecha -
            camara.up * desplazamientoAbajo;

        Gizmos.DrawWireSphere(
            origen,
            radioDeteccion
        );

        Gizmos.DrawLine(
            origen,
            origen +
            camara.forward *
            distanciaDeteccion
        );

        Gizmos.DrawWireSphere(
            origen +
            camara.forward *
            distanciaDeteccion,
            radioDeteccion
        );
    }
}