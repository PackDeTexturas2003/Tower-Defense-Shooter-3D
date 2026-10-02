using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] private float retrocesoVertical = 2f;
    [SerializeField] private float velocidadRetroceso = 20f;
    [SerializeField] private float velocidadRetorno = 10f;

    private float rotacionActual;
    private float rotacionObjetivo;

    public void AplicarRetroceso()
    {
        rotacionObjetivo += retrocesoVertical;
    }

    void Update()
    {
        rotacionObjetivo = Mathf.Lerp(rotacionObjetivo, 0f, velocidadRetorno * Time.deltaTime);

        rotacionActual = Mathf.Lerp(rotacionActual, rotacionObjetivo, velocidadRetroceso * Time.deltaTime);

        transform.localRotation *= Quaternion.Euler(-rotacionActual, 0f, 0f);
    }
}