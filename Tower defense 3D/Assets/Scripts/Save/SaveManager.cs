
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData datos;
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

        rutaArchivo = Path.Combine(
            Application.persistentDataPath,
            "SaveData.json"
        );

        Cargar();
    }

    public void Cargar()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                string json = File.ReadAllText(rutaArchivo);

                datos = JsonUtility.FromJson<SaveData>(json);

                if (datos == null)
                    datos = new SaveData();

                if (datos.armasDesbloqueadas == null)
                {
                    datos.armasDesbloqueadas =
                        new List<string>();
                }

                // Mantener la pistola como arma inicial.
                datos.DesbloquearArma("Pistola");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "Error al cargar el guardado: " + e.Message
                );

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
        if (datos == null)
            datos = new SaveData();

        try
        {
            string json = JsonUtility.ToJson(datos, true);

            File.WriteAllText(rutaArchivo, json);

            Debug.Log(
                "Guardado correctamente: " + rutaArchivo
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Error al guardar: " + e.Message
            );
        }
    }

    public bool EstaDesbloqueada(string nombreArma)
    {
        if (datos == null)
            return false;

        return datos.ArmaDesbloqueada(nombreArma);
    }

    public void DesbloquearArma(string nombreArma)
    {
        if (datos == null)
            datos = new SaveData();

        datos.DesbloquearArma(nombreArma);

        Guardar();
    }

    public int ObtenerMonedas()
    {
        if (datos == null)
            datos = new SaveData();

        return datos.monedas;
    }

    public void EstablecerMonedas(int cantidad)
    {
        if (datos == null)
            datos = new SaveData();

        datos.monedas = Mathf.Max(0, cantidad);

        Guardar();
    }

    public void GuardarTodo()
    {
        Guardar();
    }

    // Reiniciar el progreso para una nueva partida.
    public void NuevaPartida()
    {
        try
        {
            if (File.Exists(rutaArchivo))
            {
                File.Delete(rutaArchivo);
            }

            // Crear datos iniciales desde cero.
            datos = new SaveData();

            // Guardar el nuevo progreso.
            Guardar();

            Debug.Log(
                "Nueva partida: progreso reiniciado."
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Error al reiniciar la partida: " + e.Message
            );
        }
    }
}
