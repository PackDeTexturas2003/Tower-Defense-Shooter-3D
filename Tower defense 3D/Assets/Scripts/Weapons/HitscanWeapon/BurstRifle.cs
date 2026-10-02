using System.Collections;
using UnityEngine;

public class BurstRifle : HitscanWeapon
{
    [Header("Ráfaga")]
    [SerializeField] private int balasPorRafaga = 3;

    [SerializeField]
    private float tiempoEntreBalas =
        0.08f;

    private bool disparandoRafaga;

    public override void Disparar()
    {
        if (recargando)
            return;

        if (disparandoRafaga)
            return;

        if (Time.time < siguienteDisparo)
            return;

        if (balasPorRafaga <= 0)
            return;

        // Necesitamos tener suficientes balas
        // para completar toda la ráfaga.
        if (municionActual < balasPorRafaga)
        {
            Debug.Log(
                "No hay suficiente munición " +
                "para realizar la ráfaga."
            );

            return;
        }

        StartCoroutine(
            DispararRafaga()
        );

        siguienteDisparo =
            Time.time +
            tiempoEntreDisparos;
    }

    private IEnumerator DispararRafaga()
    {
        disparandoRafaga = true;

        for (int i = 0;
             i < balasPorRafaga;
             i++)
        {
            // Cada bala consume una munición.
            if (!ConsumirMunicion())
                break;

            // Cada bala es un rayo independiente.
            DispararRayo(
                camara.transform.forward
            );

            // Esperar antes de lanzar el siguiente
            // rayo de la ráfaga.
            if (i < balasPorRafaga - 1)
            {
                yield return new WaitForSeconds(
                    tiempoEntreBalas
                );
            }
        }

        // Retroceso del jugador una vez por ráfaga.
        AplicarRetrocesoJugador();

        // Retroceso de cámara una vez por ráfaga.
        if (cameraEffects != null)
        {
            cameraEffects.AgregarRecoil(
                recoilVertical,
                recoilHorizontal
            );
        }

        disparandoRafaga = false;
    }
}