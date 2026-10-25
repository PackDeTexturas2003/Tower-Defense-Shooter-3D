
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Armas")]
    [SerializeField] private Weapon[] armas;
    [SerializeField] private int armaInicial = 0;

    [Header("Audio")]
    [SerializeField] private SFXManager sfxManager;

    private Weapon[] armasOriginales;
    private Weapon armaActual;
    private int indiceArmaActual;
    private bool avisoSinMunicionReproducido;
    private bool seleccionConfigurada;

    private void Awake()
    {
        armasOriginales = armas != null
            ? (Weapon[])armas.Clone()
            : new Weapon[0];

        DesactivarTodasLasArmas();
    }

    private void Start()
    {
        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();
    }

    private void Update()
    {
        if (!seleccionConfigurada || armaActual == null)
            return;

        CambiarArma();
        ManejarRecarga();
        ManejarDisparo();
    }

    private void DesactivarTodasLasArmas()
    {
        if (armasOriginales == null)
            return;

        foreach (Weapon arma in armasOriginales)
        {
            if (arma != null)
                arma.gameObject.SetActive(false);
        }
    }

    public void ConfigurarArmasSeleccionadas(List<Weapon> seleccionadas)
    {
        if (seleccionadas == null || seleccionadas.Count == 0)
        {
            Debug.LogError(
                "WeaponManager: no se recibieron armas seleccionadas."
            );
            return;
        }

        List<Weapon> armasValidas = new List<Weapon>();

        foreach (Weapon arma in seleccionadas)
        {
            if (arma == null)
                continue;

            bool perteneceAlJugador = false;

            foreach (Weapon original in armasOriginales)
            {
                if (original == arma)
                {
                    perteneceAlJugador = true;
                    break;
                }
            }

            if (perteneceAlJugador && !armasValidas.Contains(arma))
                armasValidas.Add(arma);
        }

        if (armasValidas.Count == 0)
        {
            Debug.LogError(
                "WeaponManager: las armas seleccionadas no son válidas."
            );
            return;
        }

        DesactivarTodasLasArmas();

        armas = armasValidas.ToArray();
        armaActual = null;
        indiceArmaActual = 0;
        seleccionConfigurada = true;
        avisoSinMunicionReproducido = false;

        EquiparArma(Mathf.Clamp(armaInicial, 0, armas.Length - 1));

        Debug.Log(
            "WeaponManager: selección aplicada. Armas disponibles: " +
            armas.Length
        );
    }

    public List<AmmoType> ObtenerTiposMunicionSeleccionados()
    {
        List<AmmoType> tipos = new List<AmmoType>();

        if (!seleccionConfigurada || armas == null)
            return tipos;

        foreach (Weapon arma in armas)
        {
            if (arma == null)
                continue;

            if (!tipos.Contains(arma.TipoMunicion))
                tipos.Add(arma.TipoMunicion);
        }

        return tipos;
    }

    private void CambiarArma()
    {
        if (armaActual.EstaRecargando)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquiparArma(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquiparArma(1);

        float rueda = Input.mouseScrollDelta.y;

        if (rueda > 0f)
            CambiarArmaPorRueda(1);
        else if (rueda < 0f)
            CambiarArmaPorRueda(-1);
    }

    private void CambiarArmaPorRueda(int direccion)
    {
        if (armas == null || armas.Length <= 1)
            return;

        int nuevoIndice = indiceArmaActual;

        for (int i = 0; i < armas.Length; i++)
        {
            nuevoIndice += direccion;

            if (nuevoIndice >= armas.Length)
                nuevoIndice = 0;

            if (nuevoIndice < 0)
                nuevoIndice = armas.Length - 1;

            if (armas[nuevoIndice] != null)
            {
                EquiparArma(nuevoIndice);
                return;
            }
        }
    }

    private void EquiparArma(int indice)
    {
        if (armas == null || indice < 0 || indice >= armas.Length)
            return;

        if (armas[indice] == null || armaActual == armas[indice])
            return;

        foreach (Weapon arma in armasOriginales)
        {
            if (arma != null)
                arma.gameObject.SetActive(false);
        }

        armaActual = armas[indice];
        indiceArmaActual = indice;
        armaActual.gameObject.SetActive(true);
        avisoSinMunicionReproducido = false;

        Debug.Log("Arma equipada: " + armaActual.nombreArma);
    }

    public Weapon GetArmaActual()
    {
        return armaActual;
    }

    public bool AgregarMunicion(AmmoType tipoMunicion, int cantidad)
    {
        if (!seleccionConfigurada || armas == null)
            return false;

        foreach (Weapon arma in armas)
        {
            if (arma == null || arma.TipoMunicion != tipoMunicion)
                continue;

            bool agregada = arma.AgregarMunicion(cantidad);

            if (agregada)
            {
                avisoSinMunicionReproducido = false;
                return true;
            }
        }

        return false;
    }

    private void ManejarDisparo()
    {
        switch (armaActual.FireMode)
        {
            case FireMode.SemiAuto:
            case FireMode.Pump:
            case FireMode.Bolt:
            case FireMode.Burst:
                if (Input.GetMouseButtonDown(0))
                    Disparar();
                break;

            case FireMode.FullAuto:
                if (Input.GetMouseButton(0))
                    Disparar();
                break;
        }

        if (Input.GetMouseButtonUp(0))
            avisoSinMunicionReproducido = false;
    }

    private void Disparar()
    {
        if (armaActual == null || armaActual.EstaRecargando)
            return;

        if (!armaActual.TieneMunicion())
        {
            if (!avisoSinMunicionReproducido)
            {
                if (sfxManager != null)
                    sfxManager.ReproducirSinMunicion();

                avisoSinMunicionReproducido = true;
            }

            return;
        }

        avisoSinMunicionReproducido = false;

        int municionAntes = armaActual.MunicionActual;
        armaActual.Disparar();

        if (armaActual.MunicionActual < municionAntes &&
            sfxManager != null)
        {
            sfxManager.ReproducirDisparo(armaActual.TipoMunicion);
        }
    }

    private void ManejarRecarga()
    {
        if (!Input.GetKeyDown(KeyCode.R))
            return;

        if (armaActual == null || armaActual.EstaRecargando)
            return;

        if (armaActual.MunicionActual >= armaActual.CapacidadCargador)
            return;

        if (armaActual.MunicionReserva <= 0)
            return;

        if (sfxManager != null)
            sfxManager.ReproducirRecarga(armaActual.TipoMunicion);

        armaActual.Recargar();
    }
}
