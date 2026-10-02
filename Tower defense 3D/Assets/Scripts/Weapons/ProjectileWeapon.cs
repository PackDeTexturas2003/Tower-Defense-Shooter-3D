using UnityEngine;

public abstract class ProjectileWeapon : Weapon
{
    [Header("Proyectil")]
    [SerializeField] protected GameObject proyectilPrefab;

    [SerializeField] protected Transform puntoDisparo;

    [SerializeField] protected float velocidadProyectil = 30f;

    protected bool PuedeDispararProyectil()
    {
        if (recargando)
            return false;

        if (Time.time < siguienteDisparo)
            return false;

        if (!TieneMunicion())
        {
            Debug.Log(
                "Sin munición."
            );

            return false;
        }

        if (proyectilPrefab == null)
        {
            Debug.LogWarning(
                "No hay un prefab de proyectil asignado."
            );

            return false;
        }

        if (puntoDisparo == null)
        {
            Debug.LogWarning(
                "No hay un punto de disparo asignado."
            );

            return false;
        }

        if (!ConsumirMunicion())
            return false;

        siguienteDisparo =
            Time.time +
            tiempoEntreDisparos;

        return true;
    }

    protected virtual void CrearProyectil()
    {
        GameObject proyectil =
            Instantiate(
                proyectilPrefab,
                puntoDisparo.position,
                puntoDisparo.rotation
            );

        Rigidbody rb =
            proyectil.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                puntoDisparo.forward *
                velocidadProyectil;
        }
        else
        {
            Debug.LogWarning(
                "El proyectil no tiene Rigidbody."
            );
        }
    }
}