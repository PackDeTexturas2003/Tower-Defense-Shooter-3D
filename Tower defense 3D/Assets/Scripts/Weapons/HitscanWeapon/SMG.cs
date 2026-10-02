using UnityEngine;

public class SMG : HitscanWeapon
{
    public override void Disparar()
    {
        if (!PuedeDisparar())
            return;

        DispararRayo(
            camara.transform.forward
        );
    }
}