using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Munición")]
    [SerializeField]
    private AmmoType tipoMunicion =
        AmmoType.Pistola;

    [SerializeField]
    private int cantidadMunicion = 12;

    [Header("Audio")]
    [SerializeField]
    private SFXManager sfxManager;

    private void Start()
    {
        if (sfxManager == null)
        {
            sfxManager =
                FindFirstObjectByType<SFXManager>();
        }
    }

    // =====================================================
    // CONFIGURAR DROP
    // =====================================================

    public void Configurar(
        AmmoType tipo,
        int cantidad)
    {
        tipoMunicion = tipo;

        cantidadMunicion = cantidad;
    }

    // =====================================================
    // RECOGER MUNICIÓN
    // =====================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        WeaponManager weaponManager =
            other.GetComponentInChildren<
                WeaponManager
            >();

        if (weaponManager == null)
        {
            weaponManager =
                other.GetComponentInParent<
                    WeaponManager
                >();
        }

        if (weaponManager == null)
        {
            Debug.LogWarning(
                "No se encontró WeaponManager."
            );

            return;
        }

        bool recogida =
            weaponManager.AgregarMunicion(
                tipoMunicion,
                cantidadMunicion
            );

        if (!recogida)
        {
            Debug.Log(
                "No se pudo recoger la munición " +
                tipoMunicion +
                ". La reserva puede estar llena."
            );

            return;
        }

        // =================================================
        // SONIDO
        // =================================================

        if (sfxManager == null)
        {
            sfxManager =
                FindFirstObjectByType<SFXManager>();
        }

        if (sfxManager != null)
        {
            sfxManager.ReproducirRecogerMunicion();
        }

        Debug.Log(
            "Munición recogida: +" +
            cantidadMunicion +
            " de " +
            tipoMunicion
        );

        Destroy(gameObject);
    }
}