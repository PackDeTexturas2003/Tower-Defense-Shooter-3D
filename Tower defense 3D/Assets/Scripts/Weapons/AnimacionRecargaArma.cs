
using UnityEngine;

public class AnimacionRecargaArma : MonoBehaviour
{
    [System.Serializable]
    public class ConfiguracionArma
    {
        public AmmoType tipoArma;

        [Range(-180f, 180f)]
        public float anguloX = 0f;

        [Range(-180f, 180f)]
        public float anguloY = 0f;

        [Range(-180f, 180f)]
        public float anguloZ = 0f;
    }

    [Header("Configuración por arma")]
    [SerializeField]
    private ConfiguracionArma[] configuraciones =
    {
        new ConfiguracionArma
        {
            tipoArma = AmmoType.Pistola,
            anguloX = -100f,
            anguloY = 0f,
            anguloZ = 0f
        },

        new ConfiguracionArma
        {
            tipoArma = AmmoType.Shotgun,
            anguloX = 120f,
            anguloY = 0f,
            anguloZ = 0f
        },

        new ConfiguracionArma
        {
            tipoArma = AmmoType.SMG,
            anguloX = -80f,
            anguloY = -180f,
            anguloZ = 40f
        },

        new ConfiguracionArma
        {
            tipoArma = AmmoType.Rifle,
            anguloX = 60f,
            anguloY = 4f,
            anguloZ = -70f
        },

        new ConfiguracionArma
        {
            tipoArma = AmmoType.Sniper,
            anguloX = -60f,
            anguloY = -140f,
            anguloZ = 55f
        },

        new ConfiguracionArma
        {
            tipoArma = AmmoType.RPG,
            anguloX = -120f,
            anguloY = -30f,
            anguloZ = 90f
        }
    };

    [Header("Animación")]
    [SerializeField] private float velocidadAnimacion = 8f;

    private WeaponManager weaponManager;
    private Quaternion rotacionOriginal;

    private void Awake()
    {
        rotacionOriginal = transform.localRotation;
    }

    private void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError(
                "AnimacionRecargaArma: No se encontró WeaponManager."
            );
        }
    }

    private void Update()
    {
        if (weaponManager == null)
            return;

        Weapon armaActual = weaponManager.GetArmaActual();

        if (armaActual == null)
            return;

        Quaternion rotacionObjetivo = rotacionOriginal;

        if (armaActual.EstaRecargando)
        {
            foreach (ConfiguracionArma config in configuraciones)
            {
                if (config.tipoArma == armaActual.TipoMunicion)
                {
                    Quaternion giro = Quaternion.Euler(
                        config.anguloX,
                        config.anguloY,
                        config.anguloZ
                    );

                    rotacionObjetivo = rotacionOriginal * giro;
                    break;
                }
            }
        }

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            rotacionObjetivo,
            Time.deltaTime * velocidadAnimacion
        );
    }

    private void OnDisable()
    {
        transform.localRotation = rotacionOriginal;
    }
}
