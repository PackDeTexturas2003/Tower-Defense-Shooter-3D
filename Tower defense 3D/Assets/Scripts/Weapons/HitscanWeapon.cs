using UnityEngine;

public abstract class HitscanWeapon : Weapon
{
    [Header("Recoil Cámara")]
    [SerializeField]
    protected CameraEffects cameraEffects;

    [SerializeField]
    protected float recoilVertical = 2f;

    [SerializeField]
    protected float recoilHorizontal = 0.5f;

    private CharacterController jugadorController;

    protected override void Awake()
    {
        base.Awake();

        GameObject jugador =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (jugador != null)
        {
            jugadorController =
                jugador.GetComponent<
                    CharacterController
                >();
        }
    }

    // =====================================================
    // COMPROBAR SI PUEDE DISPARAR
    // =====================================================

    protected bool PuedeDisparar()
    {
        // No disparar durante la recarga
        if (recargando)
            return false;

        // Tiempo entre disparos
        if (Time.time < siguienteDisparo)
            return false;

        // Comprobar munición
        if (!TieneMunicion())
        {
            Debug.Log(
                "Sin munición en el cargador."
            );

            return false;
        }

        // Consumir una bala
        if (!ConsumirMunicion())
            return false;

        // Próximo disparo
        siguienteDisparo =
            Time.time +
            tiempoEntreDisparos;

        // Retroceso de cámara
        if (cameraEffects != null)
        {
            cameraEffects.AgregarRecoil(
                recoilVertical,
                recoilHorizontal
            );
        }

        return true;
    }

    // =====================================================
    // RAYCAST
    // =====================================================

    protected void DispararRayo(
        Vector3 direccion)
    {
        if (camara == null)
            return;

        Ray ray =
            new Ray(
                camara.transform.position,
                direccion
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            distancia
        ))
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

    // =====================================================
    // IMPACTO
    // =====================================================

    protected virtual void ProcesarImpacto(
        RaycastHit hit,
        Vector3 direccionDisparo)
    {
        EnemyHealth enemigo =
            hit.collider.GetComponentInParent<
                EnemyHealth
            >();

        if (enemigo == null)
            return;

        // Daño
        enemigo.RecibirDanio(
            dańo
        );

        // Empuje
        EnemyController enemyController =
            enemigo.GetComponent<
                EnemyController
            >();

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

    // =====================================================
    // RETROCESO DEL JUGADOR
    // =====================================================

    protected void AplicarRetrocesoJugador()
    {
        if (jugadorController == null)
            return;

        if (camara == null)
            return;

        Vector3 direccionRetroceso =
            -camara.transform.forward;

        direccionRetroceso.y = 0f;

        if (direccionRetroceso.sqrMagnitude <
            0.01f)
        {
            return;
        }

        direccionRetroceso.Normalize();

        jugadorController.Move(
            direccionRetroceso *
            fuerzaRetrocesoJugador
        );
    }
}