using System.Collections.Generic;
using UnityEngine;

public static class RegistroTorretasNivel
{
    private class Registro
    {
        public GameObject torre;
        public int precioPagado;
    }

    private static readonly List<Registro> registros = new List<Registro>();

    public static void Reiniciar()
    {
        registros.Clear();
    }

    public static void Registrar(GameObject torre, int precioPagado)
    {
        if (torre == null)
            return;

        registros.Add(new Registro
        {
            torre = torre,
            precioPagado = Mathf.Max(0, precioPagado)
        });
    }

    public static void ObtenerResumen(
        float porcentajeDevolucion,
        out int totalColocadas,
        out int supervivientes,
        out int dineroDevuelto)
    {
        totalColocadas = registros.Count;
        supervivientes = 0;
        dineroDevuelto = 0;

        float porcentaje = Mathf.Clamp(porcentajeDevolucion, 0f, 100f);

        foreach (Registro registro in registros)
        {
            if (registro == null || registro.torre == null)
                continue;

            TurretHealth vidaTorreta =
                registro.torre.GetComponentInChildren<TurretHealth>(true);

            if (vidaTorreta != null && vidaTorreta.VidaActual <= 0f)
                continue;

            supervivientes++;
            dineroDevuelto += Mathf.RoundToInt(
                registro.precioPagado * porcentaje / 100f
            );
        }
    }
}
