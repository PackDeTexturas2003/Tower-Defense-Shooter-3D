
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData datos;
    private SaveData copiaInicioNivel;
    private string rutaArchivo;

    public SaveData Datos => datos;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += RegistrarNivelCargado;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= RegistrarNivelCargado;
    }

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

    // Registrar el nivel cuando se carga una escena.
    private void RegistrarNivelCargado(
        Scene escena,
        LoadSceneMode modo
    )
    {
        if (escena.name == "Nivel" ||
            escena.name.StartsWith("Nivel"))
        {
            GuardarNivelContinuar(escena.name);
        }
    }

    // Guardar el nivel desde el que debe continuar.
    public void GuardarNivelContinuar(string nombreNivel)
    {
        if (string.IsNullOrWhiteSpace(nombreNivel))
            return;

        if (datos == null)
            datos = new SaveData();

        datos.ultimoNivel = nombreNivel;

        Guardar();

        Debug.Log("Nivel para continuar guardado: " + nombreNivel);
    }

    // Obtener el último nivel guardado.
    public string ObtenerNivelContinuar()
    {
        if (datos == null)
            datos = new SaveData();

        if (string.IsNullOrEmpty(datos.ultimoNivel))
            datos.ultimoNivel = "Nivel";

        return datos.ultimoNivel;
    }

    public void Cargar()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                datos = JsonUtility.FromJson<SaveData>(
                    File.ReadAllText(rutaArchivo)
                );

                if (datos == null)
                    datos = new SaveData();

                if (datos.armasDesbloqueadas == null)
                {
                    datos.armasDesbloqueadas =
                        new List<string>();
                }

                if (string.IsNullOrEmpty(datos.ultimoNivel))
                    datos.ultimoNivel = "Nivel";

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

            Debug.Log("Guardado correctamente: " + rutaArchivo);
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Error al guardar: " + e.Message
            );
        }
    }

    // Crear copia del progreso al comenzar el nivel.
    public void IniciarNivel()
    {
        if (datos == null)
            datos = new SaveData();

        copiaInicioNivel = CopiarDatos(datos);

        Debug.Log("Copia de progreso creada al iniciar nivel.");
    }

    // Confirmar progreso al ganar.
    public void ConfirmarVictoria()
    {
        copiaInicioNivel = null;

        Guardar();

        Debug.Log("Victoria: progreso confirmado.");
    }

    // Restaurar progreso al perder.
    public void RestaurarProgresoPorDerrota()
    {
        if (copiaInicioNivel == null)
        {
            Debug.LogWarning(
                "No existe copia inicial del nivel para restaurar."
            );

            return;
        }

        datos = CopiarDatos(copiaInicioNivel);

        copiaInicioNivel = null;

        Guardar();

        Debug.Log("Derrota: progreso restaurado.");
    }

    private SaveData CopiarDatos(SaveData origen)
    {
        SaveData copia = JsonUtility.FromJson<SaveData>(
            JsonUtility.ToJson(origen)
        );

        if (copia.armasDesbloqueadas == null)
        {
            copia.armasDesbloqueadas =
                new List<string>();
        }

        return copia;
    }

    public bool EstaDesbloqueada(string nombreArma)
    {
        return datos != null &&
               datos.ArmaDesbloqueada(nombreArma);
    }

    public void DesbloquearArma(string nombreArma)
    {
        if (datos == null)
            datos = new SaveData();

        datos.DesbloquearArma(nombreArma);
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
    }

    public void GuardarTodo()
    {
        Guardar();
    }

    // Reiniciar completamente la partida.
    public void NuevaPartida()
    {
        try
        {
            if (File.Exists(rutaArchivo))
                File.Delete(rutaArchivo);

            datos = new SaveData();

            copiaInicioNivel = null;

            datos.ultimoNivel = "Nivel";

            Guardar();

            Debug.Log("Nueva partida: progreso reiniciado.");
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Error al reiniciar la partida: " + e.Message
            );
        }
    }
}
