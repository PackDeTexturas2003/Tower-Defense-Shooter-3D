
using System.Collections;
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

    [Header("Configuración general")]
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

    [Header("Ráfaga del rifle")]
    [SerializeField] private int cantidadDisparosRifle = 3;

    [SerializeField] private float intervaloDisparosRifle = 0.15f;

    [Header("Sonido continuo de la SMG")]
    [SerializeField] private bool detenerSMGAlDesactivar = true;

    private AudioSource audioSource;
    private AudioSource audioSourceSMG;

    private Coroutine corrutinaRifle;
    private bool reproduciendoSMG;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volumen;

        // Fuente de audio independiente para el sonido continuo de la SMG.
        audioSourceSMG = gameObject.AddComponent<AudioSource>();
        audioSourceSMG.playOnAwake = false;
        audioSourceSMG.loop = true;
        audioSourceSMG.volume = volumen;
        audioSourceSMG.spatialBlend = audioSource.spatialBlend;
        audioSourceSMG.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
    }

    // =========================================================
    // DISPAROS
    // =========================================================

    public void ReproducirDisparo(AmmoType tipoMunicion)
    {
        switch (tipoMunicion)
        {
            case AmmoType.Pistola:
                ReproducirSonido(disparoPistola);
                break;

            case AmmoType.Shotgun:
                ReproducirSonido(disparoShotgun);
                break;

            case AmmoType.SMG:
                // Solo comienza cuando el script del arma lo solicita.
                IniciarSonidoSMG();
                break;

            case AmmoType.Rifle:
                IniciarRafagaRifle();
                break;

            case AmmoType.Sniper:
                ReproducirSonido(disparoSniper);
                break;

            case AmmoType.RPG:
                ReproducirSonido(disparoRPG);
                break;
        }
    }

    // =========================================================
    // SMG: SONIDO CONTINUO MIENTRAS SE MANTIENE EL DISPARO
    // =========================================================

    public void IniciarSonidoSMG()
    {
        if (disparoSMG == null)
            return;

        if (audioSourceSMG == null)
            return;

        // Evita reiniciar el audio en cada fotograma.
        if (reproduciendoSMG && audioSourceSMG.isPlaying)
            return;

        audioSourceSMG.clip = disparoSMG;
        audioSourceSMG.volume = volumen;
        audioSourceSMG.loop = true;

        audioSourceSMG.Play();
        reproduciendoSMG = true;
    }

    public void DetenerSonidoSMG()
    {
        if (audioSourceSMG == null)
            return;

        audioSourceSMG.Stop();
        audioSourceSMG.clip = null;
        reproduciendoSMG = false;
    }

    // =========================================================
    // RIFLE: TRES DISPAROS CONSECUTIVOS
    // =========================================================

    public void IniciarRafagaRifle()
    {
        if (disparoRifle == null)
            return;

        if (corrutinaRifle != null)
            return;

        corrutinaRifle = StartCoroutine(ReproducirRafagaRifle());
    }

    private IEnumerator ReproducirRafagaRifle()
    {
        int cantidad = Mathf.Max(1, cantidadDisparosRifle);

        for (int i = 0; i < cantidad; i++)
        {
            ReproducirSonido(disparoRifle);

            if (i < cantidad - 1)
                yield return new WaitForSeconds(
                    Mathf.Max(0f, intervaloDisparosRifle)
                );
        }

        corrutinaRifle = null;
    }

    // =========================================================
    // RECARGAS
    // =========================================================

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

    // =========================================================
    // OTROS EFECTOS DE SONIDO
    // =========================================================

    public void ReproducirSinMunicion()
    {
        ReproducirSonido(sinMunicion);
    }

    public void ReproducirImpactoEnemigo()
    {
        ReproducirSonido(impactoEnemigo, volumenImpactoEnemigo);
    }

    public void ReproducirGolpeJugador()
    {
        ReproducirSonido(golpeJugador, volumenGolpeJugador);
    }

    public void ReproducirRecogerMunicion()
    {
        ReproducirSonido(recogerMunicion, volumenRecogerMunicion);
    }

    public void ReproducirExplosionRPG()
    {
        ReproducirSonido(explosionRPG, volumenExplosionRPG);
    }

    public void ReproducirContinuar()
    {
        ReproducirSonido(sonidoContinuar, volumenContinuar);
    }

    // =========================================================
    // REPRODUCCIÓN GENERAL
    // =========================================================

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, volumen);
    }

    private void ReproducirSonido(
        AudioClip clip,
        float multiplicadorVolumen)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(
            clip,
            volumen * multiplicadorVolumen
        );
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);

        if (audioSource != null)
            audioSource.volume = volumen;

        if (audioSourceSMG != null)
            audioSourceSMG.volume = volumen;
    }

    // =========================================================
    // LIMPIEZA
    // =========================================================

    private void OnDisable()
    {
        if (detenerSMGAlDesactivar)
            DetenerSonidoSMG();

        if (corrutinaRifle != null)
        {
            StopCoroutine(corrutinaRifle);
            corrutinaRifle = null;
        }
    }
}
