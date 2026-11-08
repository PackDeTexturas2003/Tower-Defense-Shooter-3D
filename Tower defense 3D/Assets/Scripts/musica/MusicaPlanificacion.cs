
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicaPlanificacion : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup grupoMusica;

    [Header("Música")]
    [SerializeField] private AudioClip musica;

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = volumen;
        audioSource.spatialBlend = 0f;

        if (grupoMusica != null)
            audioSource.outputAudioMixerGroup = grupoMusica;
    }

    private void Start()
    {
        Reproducir();
    }

    public void Reproducir()
    {
        if (musica == null)
        {
            Debug.LogWarning(
                "MusicaPlanificacion: No hay una música asignada."
            );

            return;
        }

        audioSource.clip = musica;
        audioSource.Play();
    }

    public void Detener()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }

    public void Pausar()
    {
        if (audioSource == null)
            return;

        audioSource.Pause();
    }

    public void Continuar()
    {
        if (audioSource == null)
            return;

        audioSource.UnPause();
    }

    public void CambiarVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);
        audioSource.volume = volumen;
    }
}
