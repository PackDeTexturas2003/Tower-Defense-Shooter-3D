
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ConfiguracionSonido : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mezcladorAudio;

    [Header("Parámetros del Mixer")]
    [SerializeField] private string parametroSFX = "VolumenSFX";
    [SerializeField] private string parametroVoces = "VolumenVoces";
    [SerializeField] private string parametroMusica = "VolumenMusica";

    [Header("Slider de volumen")]
    [SerializeField] private Slider sliderSFX;
    [SerializeField] private Slider sliderVoces;
    [SerializeField] private Slider sliderMusica;

    [Header("Toggle de silencio")]
    [SerializeField] private Toggle toggleSFX;
    [SerializeField] private Toggle toggleVoces;
    [SerializeField] private Toggle toggleMusica;

    private const string CLAVE_VOLUMEN_SFX = "VolumenSFXGuardado";
    private const string CLAVE_VOLUMEN_VOCES = "VolumenVocesGuardado";
    private const string CLAVE_VOLUMEN_MUSICA = "VolumenMusicaGuardado";

    private const string CLAVE_MUTE_SFX = "MuteSFX";
    private const string CLAVE_MUTE_VOCES = "MuteVoces";
    private const string CLAVE_MUTE_MUSICA = "MuteMusica";

    private float volumenSFX = 1f;
    private float volumenVoces = 1f;
    private float volumenMusica = 1f;

    private bool muteSFX;
    private bool muteVoces;
    private bool muteMusica;

    private void Start()
    {
        CargarConfiguracion();

        ConfigurarSlider(sliderSFX);
        ConfigurarSlider(sliderVoces);
        ConfigurarSlider(sliderMusica);

        if (sliderSFX != null)
            sliderSFX.onValueChanged.AddListener(CambiarVolumenSFX);

        if (sliderVoces != null)
            sliderVoces.onValueChanged.AddListener(CambiarVolumenVoces);

        if (sliderMusica != null)
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);

        if (toggleSFX != null)
            toggleSFX.onValueChanged.AddListener(CambiarMuteSFX);

        if (toggleVoces != null)
            toggleVoces.onValueChanged.AddListener(CambiarMuteVoces);

        if (toggleMusica != null)
            toggleMusica.onValueChanged.AddListener(CambiarMuteMusica);

        ActualizarInterfaz();
        AplicarConfiguracion();
    }

    private void ConfigurarSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    // =========================================================
    // CARGAR CONFIGURACIÓN
    // =========================================================

    private void CargarConfiguracion()
    {
        volumenSFX = PlayerPrefs.GetFloat(CLAVE_VOLUMEN_SFX, 1f);
        volumenVoces = PlayerPrefs.GetFloat(CLAVE_VOLUMEN_VOCES, 1f);
        volumenMusica = PlayerPrefs.GetFloat(CLAVE_VOLUMEN_MUSICA, 1f);

        muteSFX = PlayerPrefs.GetInt(CLAVE_MUTE_SFX, 0) == 1;
        muteVoces = PlayerPrefs.GetInt(CLAVE_MUTE_VOCES, 0) == 1;
        muteMusica = PlayerPrefs.GetInt(CLAVE_MUTE_MUSICA, 0) == 1;

        volumenSFX = Mathf.Clamp01(volumenSFX);
        volumenVoces = Mathf.Clamp01(volumenVoces);
        volumenMusica = Mathf.Clamp01(volumenMusica);
    }

    // =========================================================
    // ACTUALIZAR INTERFAZ
    // =========================================================

    private void ActualizarInterfaz()
    {
        if (sliderSFX != null)
            sliderSFX.SetValueWithoutNotify(volumenSFX);

        if (sliderVoces != null)
            sliderVoces.SetValueWithoutNotify(volumenVoces);

        if (sliderMusica != null)
            sliderMusica.SetValueWithoutNotify(volumenMusica);

        if (toggleSFX != null)
            toggleSFX.SetIsOnWithoutNotify(muteSFX);

        if (toggleVoces != null)
            toggleVoces.SetIsOnWithoutNotify(muteVoces);

        if (toggleMusica != null)
            toggleMusica.SetIsOnWithoutNotify(muteMusica);
    }

    // =========================================================
    // CAMBIAR VOLUMEN
    // =========================================================

    public void CambiarVolumenSFX(float valor)
    {
        volumenSFX = Mathf.Clamp01(valor);

        PlayerPrefs.SetFloat(CLAVE_VOLUMEN_SFX, volumenSFX);

        AplicarVolumen(parametroSFX, volumenSFX, muteSFX);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenVoces(float valor)
    {
        volumenVoces = Mathf.Clamp01(valor);

        PlayerPrefs.SetFloat(CLAVE_VOLUMEN_VOCES, volumenVoces);

        AplicarVolumen(parametroVoces, volumenVoces, muteVoces);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenMusica(float valor)
    {
        volumenMusica = Mathf.Clamp01(valor);

        PlayerPrefs.SetFloat(CLAVE_VOLUMEN_MUSICA, volumenMusica);

        AplicarVolumen(parametroMusica, volumenMusica, muteMusica);
        PlayerPrefs.Save();
    }

    // =========================================================
    // CAMBIAR MUTE
    // =========================================================

    public void CambiarMuteSFX(bool activado)
    {
        muteSFX = activado;

        PlayerPrefs.SetInt(CLAVE_MUTE_SFX, muteSFX ? 1 : 0);

        AplicarVolumen(parametroSFX, volumenSFX, muteSFX);
        PlayerPrefs.Save();
    }

    public void CambiarMuteVoces(bool activado)
    {
        muteVoces = activado;

        PlayerPrefs.SetInt(CLAVE_MUTE_VOCES, muteVoces ? 1 : 0);

        AplicarVolumen(parametroVoces, volumenVoces, muteVoces);
        PlayerPrefs.Save();
    }

    public void CambiarMuteMusica(bool activado)
    {
        muteMusica = activado;

        PlayerPrefs.SetInt(CLAVE_MUTE_MUSICA, muteMusica ? 1 : 0);

        AplicarVolumen(parametroMusica, volumenMusica, muteMusica);
        PlayerPrefs.Save();
    }

    // =========================================================
    // APLICAR VOLUMEN AL AUDIO MIXER
    // =========================================================

    private void AplicarConfiguracion()
    {
        AplicarVolumen(parametroSFX, volumenSFX, muteSFX);
        AplicarVolumen(parametroVoces, volumenVoces, muteVoces);
        AplicarVolumen(parametroMusica, volumenMusica, muteMusica);
    }

    private void AplicarVolumen(
        string parametro,
        float volumen,
        bool silenciado)
    {
        if (mezcladorAudio == null)
        {
            Debug.LogWarning(
                "ConfiguracionSonido: No hay un Audio Mixer asignado."
            );

            return;
        }

        float decibelios;

        if (silenciado || volumen <= 0.0001f)
        {
            decibelios = -80f;
        }
        else
        {
            decibelios = Mathf.Log10(volumen) * 20f;
        }

        if (!mezcladorAudio.SetFloat(parametro, decibelios))
        {
            Debug.LogWarning(
                "ConfiguracionSonido: No se encontró el parámetro expuesto: "
                + parametro
            );
        }
    }

    private void OnDestroy()
    {
        if (sliderSFX != null)
            sliderSFX.onValueChanged.RemoveListener(CambiarVolumenSFX);

        if (sliderVoces != null)
            sliderVoces.onValueChanged.RemoveListener(CambiarVolumenVoces);

        if (sliderMusica != null)
            sliderMusica.onValueChanged.RemoveListener(CambiarVolumenMusica);

        if (toggleSFX != null)
            toggleSFX.onValueChanged.RemoveListener(CambiarMuteSFX);

        if (toggleVoces != null)
            toggleVoces.onValueChanged.RemoveListener(CambiarMuteVoces);

        if (toggleMusica != null)
            toggleMusica.onValueChanged.RemoveListener(CambiarMuteMusica);
    }
}
