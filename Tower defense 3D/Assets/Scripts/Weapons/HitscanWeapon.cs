using UnityEngine;

public abstract class HitscanWeapon : Weapon
{
    [Header("Recoil Cámara")]
    [SerializeField] protected CameraEffects cameraEffects;
    [SerializeField] protected float recoilVertical = 2f;
    [SerializeField] protected float recoilHorizontal = 0.5f;

    private CharacterController jugadorController;

    protected override void Awake()
    {
        base.Awake();

        GameObject jugador =
            GameObject.FindGameObjectWithTag("Player");

        if (jugador != null)
        {
            jugadorController =
                jugador.GetComponent<CharacterController>();
        }
    }

    protected bool PuedeDisparar()
    {
        if (Time.time < siguienteDisparo)
            return false;

        siguienteDisparo =
            Time.time + tiempoEntreDisparos;

        if (cameraEffects != null)
        {
            cameraEffects.AgregarRecoil(
                recoilVertical,
                recoilHorizontal
            );
        }

        return true;
    }

    protected void DispararRayo(Vector3 direccion)
    {
        Ray ray = new Ray(
            camara.transform.position,
            direccion
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            distancia))
        {
            Debug.DrawLine(
                ray.origin,
                hit.point,
                Color.red,
                1f
            );

            ProcesarImpacto(
                hit,
                direccion
            );
        }
        else
        {
            Debug.DrawRay(
                ray.origin,
                direccion * distancia,
                Color.yellow,
                1f
            );
        }
    }

    protected virtual void ProcesarImpacto(
        RaycastHit hit,
        Vector3 direccionDisparo)
    {
        // Solo buscamos enemigos
        EnemyHealth enemigo =
            hit.collider.GetComponentInParent<EnemyHealth>();

        if (enemigo == null)
            return;

        // Aplicar daño
        enemigo.RecibirDanio(dańo);

        // Aplicar empuje al enemigo
        EnemyController enemyController =
            enemigo.GetComponent<EnemyController>();

        if (enemyController != null)
        {
            enemyController.RecibirEmpuje(
                direccionDisparo,
                fuerzaEmpujeEnemigo
            );
        }

        // Retroceso del jugador
        AplicarRetrocesoJugador();
    }

    protected void AplicarRetrocesoJugador()
    {
        if (jugadorController == null)
            return;

        Vector3 direccionRetroceso =
            -camara.transform.forward;

        direccionRetroceso.y = 0f;

        if (direccionRetroceso.sqrMagnitude < 0.01f)
            return;

        direccionRetroceso.Normalize();

        jugadorController.Move(
            direccionRetroceso *
            fuerzaRetrocesoJugador
        );
    }
}