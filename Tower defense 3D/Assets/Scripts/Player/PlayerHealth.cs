using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : Health
{
    [Header("Información de Vida")]
    [SerializeField] private float mostrarVidaMaxima;
    [SerializeField] private float mostrarVidaActual;

    protected override void Awake()
    {
        base.Awake();

        ActualizarValores();
    }

    void Update()
    {
        ActualizarValores();
    }

    void ActualizarValores()
    {
        mostrarVidaMaxima = vidaMaxima;
        mostrarVidaActual = vidaActual;
    }

    protected override void Morir()
    {
        Debug.Log("El jugador ha muerto.");

        // Reinicia la escena actual
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void Curar(float cantidad)
    {
        vidaActual += cantidad;

        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        ActualizarValores();
    }
}