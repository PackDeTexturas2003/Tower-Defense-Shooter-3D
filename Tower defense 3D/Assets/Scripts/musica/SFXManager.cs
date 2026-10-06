using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    [Header("Sonidos de armas")]
    [SerializeField] private AudioClip disparoPistola;
    [SerializeField] private AudioClip disparoShotgun;
    [SerializeField] private AudioClip disparoSMG;
    [SerializeField] private AudioClip disparoRifle;
    [SerializeField] private AudioClip disparoSniper;
    [SerializeField] private AudioClip disparoRPG;

    [Header("Sonidos de recarga")]
    [SerializeField] private AudioClip recargaPistola;
    [SerializeField] private AudioClip recargaShotgun;
    [SerializeField] private AudioClip recargaSMG;
    [SerializeField] private AudioClip recargaRifle;
    [SerializeField] private AudioClip recargaSniper;
    [SerializeField] private AudioClip recargaRPG;

    [Header("Sonido sin munición")]
    [SerializeField] private AudioClip sinMunicion;

    [Header("Sonidos de combate")]
    [SerializeField] private AudioClip impactoEnemigo;
    [SerializeField] private AudioClip golpeJugador;

    [Header("Sonidos de objetos")]
    [SerializeField] private AudioClip recogerMunicion;

    [Header("Sonidos del RPG")]
    [SerializeField] private AudioClip explosionRPG;

    [Header("Sonidos de interfaz")]
    [SerializeField] private AudioClip sonidoContinuar;

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 1f;

    [Header("Volumen por tipo de sonido")]
    [Range(0f, 2f)]
    [SerializeField] private float volumenImpactoEnemigo = 1.5f;

    [Range(0f, 2f)]
    [SerializeField] private float volumenGolpeJugador = 1f;

    [Range(0f, 2f)]
    [SerializeField] private float volumenRecogerMunicion = 1f;

    [Range(0f, 2f)]
    [SerializeField] private float volumenExplosionRPG = 1f;

    [Range(0f, 2f)]
    [SerializeField] private float volumenContinuar = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volumen;
    }

    public void ReproducirDisparo(AmmoType tipoMunicion)
    {
        AudioClip sonido = null;

        switch (tipoMunicion)
        {
            case AmmoType.Pistola:
                sonido = disparoPistola;
                break;

            case AmmoType.Shotgun:
                sonido = disparoShotgun;
                break;

            case AmmoType.SMG:
                sonido = disparoSMG;
                break;

            case AmmoType.Rifle:
                sonido = disparoRifle;
                break;

            case AmmoType.Sniper:
                sonido = disparoSniper;
                break;

            case AmmoType.RPG:
                sonido = disparoRPG;
                break;
        }

        ReproducirSonido(sonido);
    }

    public void ReproducirRecarga(AmmoType tipoMunicion)
    {
        AudioClip sonido = null;

        switch (tipoMunicion)
        {
            case AmmoType.Pistola:
                sonido = recargaPistola;
                break;

            case AmmoType.Shotgun:
                sonido = recargaShotgun;
                break;

            case AmmoType.SMG:
                sonido = recargaSMG;
                break;

            case AmmoType.Rifle:
                sonido = recargaRifle;
                break;

            case AmmoType.Sniper:
                sonido = recargaSniper;
                break;

            case AmmoType.RPG:
                sonido = recargaRPG;
                break;
        }

        ReproducirSonido(sonido);
    }

    public void ReproducirSinMunicion()
    {
        ReproducirSonido(sinMunicion);
    }

    public void ReproducirImpactoEnemigo()
    {
        ReproducirSonido(
            impactoEnemigo,
            volumenImpactoEnemigo
        );
    }

    public void ReproducirGolpeJugador()
    {
        ReproducirSonido(
            golpeJugador,
            volumenGolpeJugador
        );
    }

    public void ReproducirRecogerMunicion()
    {
        ReproducirSonido(
            recogerMunicion,
            volumenRecogerMunicion
        );
    }

    public void ReproducirExplosionRPG()
    {
        ReproducirSonido(
            explosionRPG,
            volumenExplosionRPG
        );
    }

    public void ReproducirContinuar()
    {
        ReproducirSonido(
            sonidoContinuar,
            volumenContinuar
        );
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(
            clip,
            volumen
        );
    }

    private void ReproducirSonido(
        AudioClip clip,
        float multiplicadorVolumen)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(
            clip,
            volumen * multiplicadorVolumen
        );
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);
        audioSource.volume = volumen;
    }
}