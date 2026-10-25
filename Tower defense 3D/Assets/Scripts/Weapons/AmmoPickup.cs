
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Munición")]
    [SerializeField] private AmmoType tipoMunicion = AmmoType.Pistola;
    [SerializeField] private int cantidadMunicion = 12;

    [Header("Audio")]
    [SerializeField] private SFXManager sfxManager;

    // Permite consultar la cantidad configurada en el prefab.
    public int CantidadConfigurada => cantidadMunicion;

    private void Start()
    {
        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();
    }

    public void Configurar(AmmoType tipo, int cantidad)
    {
        tipoMunicion = tipo;
        cantidadMunicion = Mathf.Max(0, cantidad);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        WeaponManager weaponManager =
            other.GetComponentInChildren<WeaponManager>();

        if (weaponManager == null)
            weaponManager = other.GetComponentInParent<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogWarning(
                "AmmoPickup: No se encontró WeaponManager en el jugador."
            );
            return;
        }

        bool recogida = weaponManager.AgregarMunicion(
            tipoMunicion,
            cantidadMunicion
        );

        if (!recogida)
        {
            Debug.Log(
                "No se pudo recoger la munición " + tipoMunicion +
                ". La reserva puede estar llena o el arma no está seleccionada."
            );
            return;
        }

        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();

        if (sfxManager != null)
            sfxManager.ReproducirRecogerMunicion();

        Debug.Log(
            "Munición recogida: +" + cantidadMunicion +
            " de " + tipoMunicion
        );

        Destroy(gameObject);
    }
}
