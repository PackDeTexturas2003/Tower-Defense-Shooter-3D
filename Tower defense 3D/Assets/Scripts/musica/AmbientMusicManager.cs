
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AmbientMusicManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup grupoMusica;

    [Header("Música Ambiental")]
    [SerializeField] private AudioClip musica;

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.7f;

    [SerializeField] private bool reproducirAlIniciar = true;
    [SerializeField] private bool repetir = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = repetir;
        audioSource.volume = volumen;

        if (grupoMusica != null)
            audioSource.outputAudioMixerGroup = grupoMusica;
    }

    private void Start()
    {
        if (reproducirAlIniciar)
        {
            ReproducirMusica();
        }
    }

    public void ReproducirMusica()
    {
        if (musica == null)
        {
            Debug.LogWarning(
                "AmbientMusicManager: No hay una música asignada."
            );

            return;
        }

        ReproducirMusica(musica, repetir);
    }

    public void ReproducirMusica(AudioClip nuevaMusica, bool repetirMusica = true)
    {
        if (nuevaMusica == null)
        {
            Debug.LogWarning(
                "AmbientMusicManager: No se asignó el AudioClip que se quiere reproducir."
            );

            return;
        }

        audioSource.Stop();

        audioSource.clip = nuevaMusica;
        audioSource.loop = repetirMusica;
        audioSource.volume = volumen;

        audioSource.Play();
    }

    public void DetenerMusica()
    {
        audioSource.Stop();
    }

    public void PausarMusica()
    {
        audioSource.Pause();
    }

    public void ContinuarMusica()
    {
        audioSource.UnPause();
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);
        audioSource.volume = volumen;
    }
}
