
using UnityEngine;

public class ObjetoFlotante : MonoBehaviour
{
    [Header("Movimiento flotante")]
    [SerializeField] private float alturaFlotacion = 0.25f;
    [SerializeField] private float velocidadFlotacion = 2f;

    [Header("Rotación")]
    [SerializeField] private Vector3 ejeRotacion = new Vector3(0f, 1f, 0f);
    [SerializeField] private float velocidadRotacion = 60f;

    [Header("Brillo")]
    [SerializeField] private bool activarBrillo = true;
    [SerializeField] private Color colorBrillo = Color.yellow;
    [SerializeField] private float intensidadBrillo = 1.5f;
    [SerializeField] private float velocidadBrillo = 2f;

    private Vector3 posicionInicial;
    private Renderer[] renderizadores;
    private MaterialPropertyBlock propiedades;

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        posicionInicial = transform.position;

        renderizadores = GetComponentsInChildren<Renderer>();

        propiedades = new MaterialPropertyBlock();

        if (activarBrillo)
        {
            foreach (Renderer rend in renderizadores)
            {
                rend.material.EnableKeyword("_EMISSION");
            }
        }
    }

    private void Update()
    {
        Flotar();
        Girar();

        if (activarBrillo)
        {
            ActualizarBrillo();
        }
    }

    private void Flotar()
    {
        float movimiento = Mathf.Sin(
            Time.time * velocidadFlotacion
        ) * alturaFlotacion;

        transform.position = posicionInicial +
            Vector3.up * movimiento;
    }

    private void Girar()
    {
        transform.Rotate(
            ejeRotacion.normalized,
            velocidadRotacion * Time.deltaTime,
            Space.World
        );
    }

    private void ActualizarBrillo()
    {
        float brillo = Mathf.Lerp(
            intensidadBrillo * 0.4f,
            intensidadBrillo,
            (Mathf.Sin(Time.time * velocidadBrillo) + 1f) / 2f
        );

        foreach (Renderer rend in renderizadores)
        {
            rend.GetPropertyBlock(propiedades);

            propiedades.SetColor(
                EmissionColor,
                colorBrillo * brillo
            );

            rend.SetPropertyBlock(propiedades);
        }
    }
}
