using UnityEngine;

public class Pistol : HitscanWeapon
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