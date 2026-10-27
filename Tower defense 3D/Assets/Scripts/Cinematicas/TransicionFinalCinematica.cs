
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

[DefaultExecutionOrder(-1000)]
public class TransicionFinalCinematica : MonoBehaviour
{
    [Header("Configuración de la cinemática")]
    [SerializeField] private float duracionCinematica = 90f;
    [SerializeField] private string escenaDestino;

    [Header("Configuración para saltar")]
    [SerializeField] private Key teclaSaltar = Key.Space;
    [SerializeField] private float tiempoSalto = 60f;
    [SerializeField] private float tiempoEnfriamientoSalto = 30f;

    [Header("Texto para saltar")]
    [SerializeField] private TMP_Text textoSaltar;
    [SerializeField] private float duracionAparicion = 0.3f;
    [SerializeField] private float duracionDesaparicion = 0.3f;
    [SerializeField] private float tiempoAntesDeOcultar = 3f;
    [SerializeField] private float distanciaMinimaMovimiento = 2f;

    public float TiempoActual { get; private set; }
    public float DuracionTotal => duracionCinematica;

    private Vector2 ultimaPosicionMouse;
    private float tiempoDesdeMovimiento;
    private float alphaActual;
    private float alphaObjetivo;

    private bool cinematicTerminated;
    private bool mousePositionInitialized;

    private void Awake()
    {
        TiempoActual = 0f;
        alphaActual = 0f;
        alphaObjetivo = 0f;

        if (textoSaltar != null)
        {
            Color color = textoSaltar.color;
            color.a = 0f;
            textoSaltar.color = color;
        }

        if (Mouse.current != null)
        {
            ultimaPosicionMouse = Mouse.current.position.ReadValue();
            mousePositionInitialized = true;
        }
    }

    private void Update()
    {
        if (cinematicTerminated)
            return;

        TiempoActual += Time.unscaledDeltaTime;

        DetectarMovimientoMouse();
        ActualizarTextoSaltar();

        if (TiempoActual >= duracionCinematica)
        {
            CambiarEscena();
            return;
        }

        if (TiempoActual >= tiempoEnfriamientoSalto &&
            Keyboard.current != null &&
            Keyboard.current[teclaSaltar].wasPressedThisFrame)
        {
            SaltarCinematica();
        }
    }

    private void DetectarMovimientoMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 posicionActual = Mouse.current.position.ReadValue();

        if (!mousePositionInitialized)
        {
            ultimaPosicionMouse = posicionActual;
            mousePositionInitialized = true;
            return;
        }

        float distancia = Vector2.Distance(
            posicionActual,
            ultimaPosicionMouse
        );

        if (distancia >= distanciaMinimaMovimiento)
        {
            tiempoDesdeMovimiento = 0f;
            alphaObjetivo = 1f;
        }

        ultimaPosicionMouse = posicionActual;

        if (alphaObjetivo > 0f)
        {
            tiempoDesdeMovimiento += Time.unscaledDeltaTime;

            if (tiempoDesdeMovimiento >= tiempoAntesDeOcultar)
                alphaObjetivo = 0f;
        }
    }

    private void ActualizarTextoSaltar()
    {
        if (textoSaltar == null)
            return;

        float duracionFade = alphaObjetivo > alphaActual
            ? duracionAparicion
            : duracionDesaparicion;

        if (duracionFade <= 0f)
        {
            alphaActual = alphaObjetivo;
        }
        else
        {
            alphaActual = Mathf.MoveTowards(
                alphaActual,
                alphaObjetivo,
                Time.unscaledDeltaTime / duracionFade
            );
        }

        Color color = textoSaltar.color;
        color.a = alphaActual;
        textoSaltar.color = color;
    }

    public void SaltarCinematica()
    {
        if (TiempoActual < tiempoEnfriamientoSalto)
            return;

        alphaObjetivo = 0f;
        SaltarA(tiempoSalto);
    }

    public void SaltarA(float tiempo)
    {
        if (cinematicTerminated)
            return;

        TiempoActual = Mathf.Clamp(
            tiempo,
            0f,
            duracionCinematica
        );

        if (TiempoActual >= duracionCinematica)
            CambiarEscena();
    }

    private void CambiarEscena()
    {
        if (cinematicTerminated)
            return;

        cinematicTerminated = true;

        if (!string.IsNullOrEmpty(escenaDestino))
        {
            SceneManager.LoadScene(escenaDestino);
        }
        else
        {
            Debug.LogWarning(
                "TransicionFinalCinematica: no se ha asignado una escena de destino."
            );
        }
    }
}
