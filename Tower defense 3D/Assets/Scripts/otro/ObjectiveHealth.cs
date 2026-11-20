
using UnityEngine;

public class ObjectiveHealth : Health
{
    [Header("Game Over")]
    [SerializeField] private GameOverManager gameOverManager;

    private bool yaMurio;

    protected override void Awake()
    {
        base.Awake();

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log("¡¡EL OBJETIVO FUE DESTRUIDO!!");

        if (gameOverManager != null)
        {
            gameOverManager.MostrarDerrota(
                GameOverManager.CausaDerrota.ObjetivoDestruido
            );
        }
        else
        {
            Debug.LogError(
                "ObjectiveHealth: No se encontró GameOverManager."
            );
        }

        // Evita que el objetivo siga recibiendo daño.
        enabled = false;
    }
}
