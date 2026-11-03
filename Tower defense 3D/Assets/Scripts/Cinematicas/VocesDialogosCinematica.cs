
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VocesDialogosCinematica : MonoBehaviour
{
    [System.Serializable]
    public class VozDialogo
    {
        public AudioClip audio;

        [Min(0f)]
        public float tiempoInicio;

        [Min(0f)]
        public float tiempoFin;
    }

    [Header("Reloj compartido")]
    [SerializeField]
    private TransicionFinalCinematica reloj;

    [Header("Voces de la cinematica")]
    [SerializeField]
    private VozDialogo[] voces;

    private AudioSource audioSource;
    private int vozActual = -1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (reloj == null)
        {
            reloj = FindFirstObjectByType<TransicionFinalCinematica>();
        }

        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (reloj == null || voces == null)
        {
            DetenerVoz();
            return;
        }

        float tiempoActual = reloj.TiempoActual;
        int vozEncontrada = -1;

        for (int i = 0; i < voces.Length; i++)
        {
            VozDialogo voz = voces[i];

            if (voz == null || voz.audio == null)
                continue;

            if (voz.tiempoFin <= voz.tiempoInicio)
                continue;

            if (tiempoActual >= voz.tiempoInicio &&
                tiempoActual < voz.tiempoFin)
            {
                vozEncontrada = i;
                break;
            }
        }

        if (vozEncontrada == -1)
        {
            DetenerVoz();
            return;
        }

        if (vozActual == vozEncontrada && audioSource.isPlaying)
            return;

        ReproducirVoz(vozEncontrada, tiempoActual);
    }

    private void ReproducirVoz(int indice, float tiempoActual)
    {
        VozDialogo voz = voces[indice];

        audioSource.Stop();
        audioSource.clip = voz.audio;

        float tiempoTranscurrido = tiempoActual - voz.tiempoInicio;
        float duracionClip = voz.audio.length;

        if (tiempoTranscurrido >= duracionClip)
        {
            vozActual = indice;
            return;
        }

        audioSource.time = Mathf.Clamp(
            tiempoTranscurrido,
            0f,
            Mathf.Max(0f, duracionClip - 0.01f)
        );

        vozActual = indice;
        audioSource.Play();
    }

    private void DetenerVoz()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        vozActual = -1;
    }

    public void DetenerVoces()
    {
        DetenerVoz();
    }

    public void ReiniciarVoces()
    {
        DetenerVoz();
    }
}
