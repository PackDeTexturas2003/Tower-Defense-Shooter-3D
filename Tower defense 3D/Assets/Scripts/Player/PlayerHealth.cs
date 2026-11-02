
using UnityEngine;

public class PlayerHealth : Health
{
    [Header("Información de Vida")]
    [SerializeField] private float mostrarVidaMaxima;
    [SerializeField] private float mostrarVidaActual;

    [Header("Game Over")]
    [SerializeField] private GameOverManager gameOverManager;

    private bool yaMurio;

    protected override void Awake()
    {
        base.Awake();

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();

        ActualizarValores();
    }

    private void Update()
    {
        ActualizarValores();
    }

    private void ActualizarValores()
    {
        mostrarVidaMaxima = vidaMaxima;
        mostrarVidaActual = vidaActual;
    }

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log("El jugador ha muerto.");

        if (gameOverManager != null)
        {
            gameOverManager.MostrarDerrota(
                GameOverManager.CausaDerrota.JugadorEliminado
            );
        }
        else
        {
            Debug.LogError(
                "PlayerHealth: No se encontró GameOverManager."
            );
        }
    }

    public void Curar(float cantidad)
    {
        if (yaMurio || cantidad <= 0f)
            return;

        vidaActual += cantidad;

        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        ActualizarValores();
    }
}
