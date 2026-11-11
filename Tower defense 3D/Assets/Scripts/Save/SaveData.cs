
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int monedas = 100;

    public List<string> armasDesbloqueadas = new List<string>();

    public string ultimoNivel = "Nivel";

    public SaveData()
    {
        DesbloquearArma("Pistola");
    }

    public bool ArmaDesbloqueada(string nombreArma)
    {
        return armasDesbloqueadas.Contains(nombreArma);
    }

    public void DesbloquearArma(string nombreArma)
    {
        if (string.IsNullOrEmpty(nombreArma))
            return;

        if (!armasDesbloqueadas.Contains(nombreArma))
        {
            armasDesbloqueadas.Add(nombreArma);
        }
    }
}
