using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltRifle : HitscanWeapon
{
    [Header("Cerrojo")]
    [SerializeField] private float tiempoCerrojo = 1f;

    [Header("Penetracion")]
    [SerializeField] private int enemigosQuePuedeAtravesar = 3;

    private bool accionandoCerrojo;

    public override void Disparar()
    {
        if (accionandoCerrojo)
            return;

        if (!PuedeDisparar())
            return;

        DispararConPenetracion();

        StartCoroutine(
            AccionarCerrojo()
        );
    }

    private void DispararConPenetracion()
    {
        if (camara == null)
            return;

        Vector3 origen =
            camara.transform.position;

        Vector3 direccion =
            camara.transform.forward;

        Ray ray =
            new Ray(
                origen,
                direccion
            );

        RaycastHit[] impactos =
            Physics.RaycastAll(
                ray,
                distancia,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

        Array.Sort(
            impactos,
            (a, b) =>
                a.distance.CompareTo(
                    b.distance
                )
        );

        HashSet<EnemyHealth> enemigosImpactados =
            new HashSet<EnemyHealth>();

        int enemigosAtravesados = 0;

        foreach (RaycastHit impacto in impactos)
        {
            Debug.DrawLine(
                origen,
                impacto.point,
                Color.red,
                1f
            );

            EnemyHealth enemigo =
                impacto.collider.GetComponentInParent<EnemyHealth>();

            // ========================================
            // ENEMIGO
            // ========================================

            if (enemigo != null)
            {
                // Evita golpear dos veces al mismo enemigo
                // si tiene varios colliders.
                if (enemigosImpactados.Contains(enemigo))
                    continue;

                enemigosImpactados.Add(
                    enemigo
                );

                enemigo.RecibirDanio(
                    dańo
                );

                EnemyController enemyController =
                    enemigo.GetComponent<EnemyController>();

                if (enemyController != null)
                {
                    enemyController.RecibirEmpuje(
                        direccion,
                        fuerzaEmpujeEnemigo
                    );
                }

                enemigosAtravesados++;

                Debug.Log(
                    "BoltRifle atraveso enemigo: " +
                    enemigo.gameObject.name +
                    " | " +
                    enemigosAtravesados +
                    "/" +
                    enemigosQuePuedeAtravesar
                );

                // Ya atraveso la cantidad maxima
                // de enemigos.
                if (enemigosAtravesados >=
                    enemigosQuePuedeAtravesar)
                {
                    break;
                }

                // Continua buscando el siguiente enemigo.
                continue;
            }

            // ========================================
            // OBSTACULO
            // ========================================

            Debug.Log(
                "BoltRifle detenido por: " +
                impacto.collider.gameObject.name
            );

            break;
        }
    }

    private IEnumerator AccionarCerrojo()
    {
        accionandoCerrojo = true;

        Debug.Log(
            "Cerrojo del francotirador..."
        );

        yield return new WaitForSeconds(
            tiempoCerrojo
        );

        accionandoCerrojo = false;

        Debug.Log(
            "Francotirador listo para disparar."
        );
    }
}