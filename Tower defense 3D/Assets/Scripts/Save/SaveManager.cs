using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData datos;
    private SaveData copiaInicioNivel;
    private string rutaArchivo;

    public SaveData Datos => datos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        rutaArchivo = Path.Combine(Application.persistentDataPath, "SaveData.json");
        Cargar();
    }

    public void Cargar()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                datos = JsonUtility.FromJson<SaveData>(File.ReadAllText(rutaArchivo));
                if (datos == null) datos = new SaveData();
                if (datos.armasDesbloqueadas == null)
                    datos.armasDesbloqueadas = new List<string>();

                datos.DesbloquearArma("Pistola");
            }
            catch (Exception e)
            {
                Debug.LogError("Error al cargar el guardado: " + e.Message);
                datos = new SaveData();
            }
        }
        else
        {
            datos = new SaveData();
        }

        Guardar();
    }

    public void Guardar()
    {
        if (datos == null) datos = new SaveData();

        try
        {
            string json = JsonUtility.ToJson(datos, true);
            File.WriteAllText(rutaArchivo, json);
            Debug.Log("Guardado correctamente: " + rutaArchivo);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al guardar: " + e.Message);
        }
    }

    // Invocar al comenzar la fase jugable del nivel, después de cargar los datos.
    public void IniciarNivel()
    {
        if (datos == null) datos = new SaveData();
        copiaInicioNivel = CopiarDatos(datos);
        Debug.Log("Copia de progreso creada al iniciar nivel.");
    }

    // Ganar confirma los cambios temporales realizados en el nivel.
    public void ConfirmarVictoria()
    {
        copiaInicioNivel = null;
        Guardar();
        Debug.Log("Victoria: progreso del nivel confirmado.");
    }

    // Perder restaura exactamente el progreso que había antes de este nivel.
    public void RestaurarProgresoPorDerrota()
    {
        if (copiaInicioNivel == null)
        {
            Debug.LogWarning("No existe copia inicial del nivel para restaurar.");
            return;
        }

        datos = CopiarDatos(copiaInicioNivel);
        copiaInicioNivel = null;
        Guardar();
        Debug.Log("Derrota: progreso previo al nivel restaurado.");
    }

    private SaveData CopiarDatos(SaveData origen)
    {
        SaveData copia = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(origen));
        if (copia.armasDesbloqueadas == null)
            copia.armasDesbloqueadas = new List<string>();
        return copia;
    }

    public bool EstaDesbloqueada(string nombreArma)
    {
        return datos != null && datos.ArmaDesbloqueada(nombreArma);
    }

    // Cambia solo los datos en memoria durante el nivel.
    public void DesbloquearArma(string nombreArma)
    {
        if (datos == null) datos = new SaveData();
        datos.DesbloquearArma(nombreArma);
    }

    public int ObtenerMonedas()
    {
        if (datos == null) datos = new SaveData();
        return datos.monedas;
    }

    // Cambia solo las monedas en memoria; Guardar se ejecuta al ganar/perder.
    public void EstablecerMonedas(int cantidad)
    {
        if (datos == null) datos = new SaveData();
        datos.monedas = Mathf.Max(0, cantidad);
    }

    public void GuardarTodo()
    {
        Guardar();
    }

    public void NuevaPartida()
    {
        try
        {
            if (File.Exists(rutaArchivo))
                File.Delete(rutaArchivo);

            datos = new SaveData();
            copiaInicioNivel = null;
            Guardar();
            Debug.Log("Nueva partida: progreso reiniciado.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al reiniciar la partida: " + e.Message);
        }
    }
}
