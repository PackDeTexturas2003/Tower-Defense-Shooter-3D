using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;

    [Header("Recoil")]
    [SerializeField] private float velocidadRetorno = 10f;
    [SerializeField] private float velocidadAplicacion = 20f;

    private Vector3 recoilActual;
    private Vector3 recoilObjetivo;

    private Quaternion rotacionBase;

    private void Start()
    {
        if (cameraTransform != null)
            rotacionBase = cameraTransform.localRotation;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        recoilObjetivo = Vector3.Lerp(
            recoilObjetivo,
            Vector3.zero,
            velocidadRetorno * Time.deltaTime
        );

        recoilActual = Vector3.Lerp(
            recoilActual,
            recoilObjetivo,
            velocidadAplicacion * Time.deltaTime
        );

        cameraTransform.localRotation =
            rotacionBase *
            Quaternion.Euler(recoilActual);
    }

    public void AgregarRecoil(float vertical, float horizontal)
    {
        recoilObjetivo += new Vector3(
            -vertical,
            Random.Range(-horizontal, horizontal),
            0f
        );
    }

    public void ReiniciarRecoil()
    {
        recoilActual = Vector3.zero;
        recoilObjetivo = Vector3.zero;

        if (cameraTransform != null)
            cameraTransform.localRotation = rotacionBase;
    }
}