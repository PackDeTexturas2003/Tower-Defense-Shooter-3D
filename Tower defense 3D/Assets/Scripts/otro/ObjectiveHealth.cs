using UnityEngine;

public class ObjectiveHealth : Health
{
    private bool yaMurio = false;

    protected override void Morir()
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log(
            "¡¡EL OBJETIVO FUE DESTRUIDO!!"
        );

        // Por ahora solamente desactivamos
        // el componente de vida.
        // Después podemos poner aquí
        // la derrota de la partida.

        enabled = false;
    }
}