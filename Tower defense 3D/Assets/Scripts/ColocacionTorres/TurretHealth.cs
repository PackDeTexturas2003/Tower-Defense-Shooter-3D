using UnityEngine;

public class TurretHealth : Health
{
    [Header("Configuración de muerte")]
    [SerializeField] private bool destruirAlMorir = true;

    private bool yaMurio = false;

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log($"¡La torreta {gameObject.name} ha sido destruida!");

        if (destruirAlMorir)
        {
            Destroy(gameObject);
        }
        else
        {
            // Permite conservar la torreta en escena
            // si posteriormente quieres gestionar su
            // destrucción de otra manera.
            enabled = false;
        }
    }
}