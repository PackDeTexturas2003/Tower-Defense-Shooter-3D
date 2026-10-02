using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Armas")]
    [SerializeField]
    private Weapon[] armas;

    [SerializeField]
    private int armaInicial = 0;

    [Header("Audio")]
    [SerializeField]
    private SFXManager sfxManager;

    private Weapon armaActual;

    private int indiceArmaActual;

    private bool avisoSinMunicionReproducido;

    private void Start()
    {
        EquiparArma(armaInicial);

        if (sfxManager == null)
        {
            sfxManager =
                FindFirstObjectByType<SFXManager>();
        }
    }

    private void Update()
    {
        if (armaActual == null)
            return;

        CambiarArma();

        ManejarRecarga();

        ManejarDisparo();
    }

    // =====================================================
    // CAMBIO DE ARMA
    // =====================================================

    private void CambiarArma()
    {
        if (armaActual.EstaRecargando)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquiparArma(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquiparArma(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            EquiparArma(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            EquiparArma(3);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            EquiparArma(4);

        if (Input.GetKeyDown(KeyCode.Alpha6))
            EquiparArma(5);

        float rueda =
            Input.mouseScrollDelta.y;

        if (rueda > 0f)
        {
            CambiarArmaPorRueda(1);
        }
        else if (rueda < 0f)
        {
            CambiarArmaPorRueda(-1);
        }
    }

    // =====================================================
    // CAMBIO CON RUEDA
    // =====================================================

    private void CambiarArmaPorRueda(
        int direccion)
    {
        if (armas == null ||
            armas.Length == 0)
        {
            return;
        }

        int nuevoIndice =
            indiceArmaActual +
            direccion;

        if (nuevoIndice >= armas.Length)
        {
            nuevoIndice = 0;
        }

        if (nuevoIndice < 0)
        {
            nuevoIndice =
                armas.Length - 1;
        }

        for (int i = 0;
             i < armas.Length;
             i++)
        {
            if (armas[nuevoIndice] != null)
            {
                EquiparArma(
                    nuevoIndice
                );

                return;
            }

            nuevoIndice += direccion;

            if (nuevoIndice >= armas.Length)
            {
                nuevoIndice = 0;
            }

            if (nuevoIndice < 0)
            {
                nuevoIndice =
                    armas.Length - 1;
            }
        }
    }

    // =====================================================
    // EQUIPAR ARMA
    // =====================================================

    private void EquiparArma(
        int indice)
    {
        if (armas == null)
            return;

        if (indice < 0 ||
            indice >= armas.Length)
        {
            return;
        }

        if (armas[indice] == null)
            return;

        if (armaActual == armas[indice])
            return;

        for (int i = 0;
             i < armas.Length;
             i++)
        {
            if (armas[i] != null)
            {
                armas[i].gameObject.SetActive(
                    false
                );
            }
        }

        armaActual =
            armas[indice];

        indiceArmaActual =
            indice;

        armaActual.gameObject.SetActive(
            true
        );

        avisoSinMunicionReproducido =
            false;

        Debug.Log(
            "Arma equipada: " +
            armaActual.nombreArma
        );
    }

    // =====================================================
    // OBTENER ARMA ACTUAL
    // =====================================================

    public Weapon GetArmaActual()
    {
        return armaActual;
    }

    // =====================================================
    // AGREGAR MUNICIÓN
    // =====================================================

    public bool AgregarMunicion(
        AmmoType tipoMunicion,
        int cantidad)
    {
        if (armas == null)
            return false;

        for (int i = 0;
             i < armas.Length;
             i++)
        {
            if (armas[i] == null)
                continue;

            if (armas[i].TipoMunicion !=
                tipoMunicion)
            {
                continue;
            }

            bool agregada =
                armas[i].AgregarMunicion(
                    cantidad
                );

            if (agregada)
            {
                avisoSinMunicionReproducido =
                    false;

                return true;
            }
        }

        return false;
    }

    // =====================================================
    // DISPARO
    // =====================================================

    private void ManejarDisparo()
    {
        switch (armaActual.FireMode)
        {
            case FireMode.SemiAuto:

            case FireMode.Pump:

            case FireMode.Bolt:

                if (Input.GetMouseButtonDown(0))
                {
                    Disparar();
                }

                break;

            case FireMode.FullAuto:

                if (Input.GetMouseButton(0))
                {
                    Disparar();
                }

                break;

            case FireMode.Burst:

                if (Input.GetMouseButtonDown(0))
                {
                    Disparar();
                }

                break;
        }

        if (Input.GetMouseButtonUp(0))
        {
            avisoSinMunicionReproducido =
                false;
        }
    }

    private void Disparar()
    {
        if (armaActual.EstaRecargando)
            return;

        if (!armaActual.TieneMunicion())
        {
            if (!avisoSinMunicionReproducido)
            {
                if (sfxManager != null)
                {
                    sfxManager.ReproducirSinMunicion();
                }

                avisoSinMunicionReproducido =
                    true;
            }

            return;
        }

        avisoSinMunicionReproducido =
            false;

        int municionAntes =
            armaActual.MunicionActual;

        armaActual.Disparar();

        if (armaActual.MunicionActual <
            municionAntes)
        {
            if (sfxManager != null)
            {
                sfxManager.ReproducirDisparo(
                    armaActual.TipoMunicion
                );
            }
        }
    }

    // =====================================================
    // RECARGA
    // =====================================================

    private void ManejarRecarga()
    {
        if (!Input.GetKeyDown(KeyCode.R))
            return;

        if (armaActual.EstaRecargando)
            return;

        if (armaActual.MunicionActual >=
            armaActual.CapacidadCargador)
        {
            return;
        }

        if (armaActual.MunicionReserva <= 0)
            return;

        if (sfxManager != null)
        {
            sfxManager.ReproducirRecarga(
                armaActual.TipoMunicion
            );
        }

        armaActual.Recargar();
    }
}