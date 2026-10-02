using UnityEngine;

public abstract class Health : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] protected float vidaMaxima = 100f;

    protected float vidaActual;

    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;

    protected virtual void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public virtual void RecibirDanio(float cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual < 0)
            vidaActual = 0;

        Debug.Log($"{gameObject.name} recibió {cantidad} de daño. Vida: {vidaActual}");

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    protected abstract void Morir();
}