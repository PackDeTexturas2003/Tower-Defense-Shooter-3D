
using UnityEngine;
using TMPro;

public class DineroJugador : MonoBehaviour
{
    [Header("Dinero")]
    [SerializeField] private int dineroInicial = 100;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text textoDinero;

    private int dineroActual;

    private void Awake()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.IniciarNivel();
        if (SaveManager.Instance != null)
        {
            dineroActual = SaveManager.Instance.ObtenerMonedas();
        }
        else
        {
            dineroActual = dineroInicial;
        }

        ActualizarTextoDinero();
    }

    public int ObtenerDinero()
    {
        return dineroActual;
    }

    public bool TieneDinero(int cantidad)
    {
        return dineroActual >= cantidad;
    }

    public bool GastarDinero(int cantidad)
    {
        if (cantidad < 0)
            return false;

        if (!TieneDinero(cantidad))
            return false;

        dineroActual -= cantidad;

        ActualizarTextoDinero();
        GuardarDinero();

        Debug.Log(
            "Dinero gastado: " + cantidad +
            ". Dinero restante: " + dineroActual
        );

        return true;
    }

    public void AgregarDinero(int cantidad)
    {
        if (cantidad <= 0)
            return;

        dineroActual += cantidad;

        ActualizarTextoDinero();
        GuardarDinero();

        Debug.Log(
            "Dinero recibido: " + cantidad +
            ". Dinero actual: " + dineroActual
        );
    }

    private void GuardarDinero()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.EstablecerMonedas(
                dineroActual
            );
        }
    }

    private void ActualizarTextoDinero()
    {
        if (textoDinero != null)
        {
            textoDinero.text = "$" + dineroActual;
        }
    }
}
