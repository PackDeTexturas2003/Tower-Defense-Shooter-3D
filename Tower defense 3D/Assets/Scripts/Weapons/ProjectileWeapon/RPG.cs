using UnityEngine;

public class RPG : ProjectileWeapon
{
    [Header("Explosión")]
    [SerializeField] private float radioExplosion = 5f;

    [SerializeField] private float fuerzaExplosion = 10f;

    public override void Disparar()
    {
        if (!PuedeDispararProyectil())
            return;

        CrearProyectil();

        AplicarRetroceso();
    }

    protected override void CrearProyectil()
    {
        GameObject proyectil =
            Instantiate(
                proyectilPrefab,
                puntoDisparo.position,
                puntoDisparo.rotation
            );

        RPGProjectile rpgProjectile =
            proyectil.GetComponent<RPGProjectile>();

        if (rpgProjectile != null)
        {
            rpgProjectile.Configurar(
                dańo,
                radioExplosion,
                fuerzaExplosion
            );
        }

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
                "El proyectil del RPG no tiene Rigidbody."
            );
        }
    }

    private void AplicarRetroceso()
    {
        GameObject jugador =
            GameObject.FindGameObjectWithTag("Player");

        if (jugador == null)
            return;

        CharacterController controller =
            jugador.GetComponent<CharacterController>();

        if (controller == null)
            return;

        Vector3 direccion =
            -camara.transform.forward;

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        direccion.Normalize();

        controller.Move(
            direccion *
            fuerzaRetrocesoJugador
        );
    }
}