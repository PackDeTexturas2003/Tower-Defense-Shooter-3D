using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private TMP_Text textoMunicion;

    void Update()
    {
        if (weaponManager == null || textoMunicion == null)
            return;

        Weapon arma = weaponManager.GetArmaActual();

        if (arma == null)
        {
            textoMunicion.text = "";
            return;
        }

        textoMunicion.text =
            arma.MunicionActual +
            " / " +
            arma.MunicionReserva;
    }
}