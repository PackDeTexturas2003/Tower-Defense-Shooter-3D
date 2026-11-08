
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicaCinematica : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup grupoMusica;

    [Header("Configuración de música")]
    [SerializeField] private AudioClip cancion;
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.5f;
    [SerializeField] private bool repetir = true;
    [SerializeField] private bool reproducirAlIniciar = true;

    [Header("Desvanecimiento")]
    [SerializeField] private bool desvanecerAlFinal = false;
    [Min(0f)]
    [SerializeField] private float duracionDesvanecimiento = 2f;

    [Header("Reloj de la cinemática")]
    [SerializeField] private TransicionFinalCinematica reloj;

    private AudioSource fuenteAudio;
    private float tiempoInicioDesvanecimiento;
    private float volumenInicial;
    private bool desvanecimientoIniciado;
    private bool musicaTerminada;

    private void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();

        if (reloj == null)
        {
            reloj = FindFirstObjectByType<TransicionFinalCinematica>();
        }

        fuenteAudio.playOnAwake = false;
        fuenteAudio.loop = repetir;
        fuenteAudio.volume = volumen;
        fuenteAudio.spatialBlend = 0f;
        fuenteAudio.clip = cancion;

        if (grupoMusica != null)
            fuenteAudio.outputAudioMixerGroup = grupoMusica;

        volumenInicial = volumen;
    }

    private void Start()
    {
        if (reproducirAlIniciar && cancion != null)
        {
            fuenteAudio.Play();
        }
    }

    private void Update()
    {
        if (musicaTerminada || !desvanecerAlFinal || reloj == null)
            return;

        if (reloj.TiempoActual >= reloj.DuracionTotal - duracionDesvanecimiento)
        {
            if (!desvanecimientoIniciado)
            {
                desvanecimientoIniciado = true;
                tiempoInicioDesvanecimiento = reloj.TiempoActual;
            }

            float tiempoTranscurrido =
                reloj.TiempoActual - tiempoInicioDesvanecimiento;

            float progreso = duracionDesvanecimiento > 0f
                ? Mathf.Clamp01(tiempoTranscurrido / duracionDesvanecimiento)
                : 1f;

            fuenteAudio.volume = Mathf.Lerp(
                volumenInicial,
                0f,
                progreso
            );

            if (progreso >= 1f)
            {
                fuenteAudio.Stop();
                musicaTerminada = true;
            }
        }
    }

    public void ReproducirMusica()
    {
        if (cancion == null)
            return;

        musicaTerminada = false;
        desvanecimientoIniciado = false;
        fuenteAudio.clip = cancion;
        fuenteAudio.loop = repetir;
        fuenteAudio.volume = volumen;
        fuenteAudio.Play();
    }

    public void DetenerMusica()
    {
        fuenteAudio.Stop();
    }

    public void EstablecerVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);
        volumenInicial = volumen;

        if (fuenteAudio != null && !desvanecimientoIniciado)
        {
            fuenteAudio.volume = volumen;
        }
    }
}
