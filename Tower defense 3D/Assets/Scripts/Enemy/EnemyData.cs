using UnityEngine;

[CreateAssetMenu(
    fileName = "NuevoEnemigo",
    menuName = "Enemigos/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Información")]
    public string nombreEnemigo = "Enemigo";

    [Header("Movimiento")]
    public float velocidad = 3f;

    [Header("Detección")]
    public float radioDeteccion = 10f;

    [Header("Ataque")]
    public float distanciaAtaque = 2f;
    public float dano = 10f;
    public float tiempoEntreAtaques = 1f;
}