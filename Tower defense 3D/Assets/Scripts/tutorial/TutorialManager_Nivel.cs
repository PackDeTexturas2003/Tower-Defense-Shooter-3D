
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager_Nivel : MonoBehaviour
{
    [Header("Panel de selección de armas")]
    [SerializeField] private GameObject panelSeleccionArmas;

    [Header("Paneles del tutorial (12 elementos)")]
    [SerializeField] private GameObject[] paneles = new GameObject[12];

    [Header("Audios del tutorial (12 elementos)")]
    [SerializeField] private AudioClip[] audios = new AudioClip[12];

    [Header("AudioSource")]
    [SerializeField] private AudioSource audioSource;

    [Header("Espera para mostrar el panel 9 después del primer enemigo")]
    [SerializeField] private float esperaAntesPanel9 = 7f;

    [Header("Controles del jugador")]
    [SerializeField] private Movimiento movimiento;
    [SerializeField] private Esquive esquive;
    [SerializeField] private Salto salto;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private WeaponManager weaponManager;

    private int indicePanelActual = -1;

    private bool tutorialActivo;
    private bool tutorialFinalizado;
    private bool esperandoPrimerEnemigo;
    private bool primerEnemigoDetectado;
    private bool esperandoConfirmacionArmas;
    private bool controlesGuardados;

    private bool movimientoEstabaActivo;
    private bool esquiveEstabaActivo;
    private bool saltoEstabaActivo;
    private bool mouseLookEstabaActivo;
    private bool weaponManagerEstabaActivo;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private float escalaTiempoAnterior = 1f;

    private Coroutine corrutinaTutorialEnemigo;

    public bool TutorialActivo => tutorialActivo;

    private void Awake()
    {
        for (int i = 0; i < paneles.Length; i++)
        {
            if (paneles[i] != null)
                paneles[i].SetActive(false);
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        // Conectar los botones Continuar.
        for (int i = 0; i < paneles.Length; i++)
        {
            if (paneles[i] == null)
            {
                Debug.LogError(
                    "Falta asignar el panel " + (i + 1), this);
                continue;
            }

            Button boton =
                paneles[i].GetComponentInChildren<Button>(true);

            if (boton != null)
            {
                boton.onClick.RemoveListener(Continuar);
                boton.onClick.AddListener(Continuar);
            }
            else
            {
                Debug.LogWarning(
                    "El panel " + (i + 1) +
                    " no tiene un botón Button.", paneles[i]);
            }
        }
    }

    // =====================================
    // INICIAR TUTORIAL
    // =====================================

    public void IniciarTutorial()
    {
        if (tutorialFinalizado || tutorialActivo)
            return;

        Debug.Log("Tutorial: iniciando primera conversación.");

        if (panelSeleccionArmas == null)
        {
            Debug.LogError(
                "No está asignado PanelSeleccionArmas.", this);
        }
        else
        {
            panelSeleccionArmas.SetActive(true);
        }

        MostrarPanel(0);
    }

    // =====================================
    // MOSTRAR PANEL
    // =====================================

    private void MostrarPanel(int indice)
    {
        if (tutorialFinalizado ||
            indice < 0 ||
            indice >= paneles.Length)
            return;

        if (paneles[indice] == null)
        {
            Debug.LogError(
                "No está asignado el panel " + (indice + 1), this);
            return;
        }

        BloquearJuego();

        // Mostrar solamente el panel correspondiente.
        for (int i = 0; i < paneles.Length; i++)
        {
            if (paneles[i] != null)
                paneles[i].SetActive(i == indice);
        }

        GameObject panelActual = paneles[indice];

        // Reactivar los elementos visuales del panel seleccionado.
        // Incluye Image, Text y TextMeshProUGUI (todos heredan de Graphic).
        // Se usa true para incluir también los componentes de hijos inactivos.
        Graphic[] elementosVisuales =
            panelActual.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic elemento in elementosVisuales)
        {
            elemento.enabled = true;
        }

        // Colocar el panel del tutorial encima de la selección.
        panelActual.transform.SetAsLastSibling();

        // Canvas propio para dibujar el tutorial encima de la seleccion.
        Canvas canvasPropio = panelActual.GetComponent<Canvas>();
        if (canvasPropio == null)
            canvasPropio = panelActual.AddComponent<Canvas>();
        canvasPropio.overrideSorting = true;
        canvasPropio.sortingOrder = 100;

        if (panelActual.GetComponent<GraphicRaycaster>() == null)
            panelActual.AddComponent<GraphicRaycaster>();

        CanvasGroup grupo = panelActual.GetComponent<CanvasGroup>();
        if (grupo == null)
            grupo = panelActual.AddComponent<CanvasGroup>();
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;

        Canvas canvas = panelActual.GetComponentInParent<Canvas>();

        Debug.Log(
            "Panel tutorial activado: " + panelActual.name, panelActual);

        Debug.Log(
            "Está activo en jerarquía: " +
            panelActual.activeInHierarchy, panelActual);

        if (canvas != null)
        {
            Debug.Log(
                "Canvas del tutorial: " + canvas.name, canvas);
        }
        else
        {
            Debug.LogError(
                "El panel no tiene un Canvas en sus padres.", panelActual);
        }

        indicePanelActual = indice;
        tutorialActivo = true;

        // Reproducir audio.
        audioSource.Stop();
        audioSource.clip = null;

        if (audios != null &&
            indice < audios.Length &&
            audios[indice] != null)
        {
            audioSource.clip = audios[indice];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning(
                "Falta el audio del panel " + (indice + 1), this);
        }

        Debug.Log(
            "Tutorial mostrando panel " + (indice + 1), this);
    }

    // =====================================
    // BOTÓN CONTINUAR
    // =====================================

    public void Continuar()
    {
        if (!tutorialActivo || indicePanelActual < 0)
            return;

        audioSource.Stop();

        paneles[indicePanelActual].SetActive(false);
        tutorialActivo = false;

        // Después del panel 4, dejar que el jugador termine la selección
        // de armas. El panel 5 se mostrará cuando se cierre esa selección.
        if (indicePanelActual == 3)
        {
            esperandoConfirmacionArmas = true;

            Debug.Log(
                "Tutorial: panel 4 terminado. Esperando confirmación de armas para mostrar el panel 5.");

            return;
        }

        // Después del panel 7, permitir que el jugador avance
        // hasta el combate y esperar al primer enemigo.
        // El panel 9 aparecerá 7 segundos después de detectarlo.
        if (indicePanelActual == 6)
        {
            ReanudarControlesManteniendoPausa();

            esperandoPrimerEnemigo = true;

            if (primerEnemigoDetectado)
                IniciarEsperaEnemigo();

            Debug.Log(
                "Tutorial: panel 7 terminado. Esperando primer enemigo para mostrar el panel 9.");

            return;
        }

        // Los demás paneles avanzan normalmente.
        if (indicePanelActual < 11)
        {
            MostrarPanel(indicePanelActual + 1);
            return;
        }

        // Panel 12: finalizar.
        FinalizarTutorial();
    }

    // =====================================
    // MOSTRAR PANEL 5 DESPUÉS DE SELECCIONAR ARMAS
    // =====================================

    public void MostrarPanelDespuesSeleccionArmas()
    {
        if (tutorialFinalizado || !esperandoConfirmacionArmas)
            return;

        esperandoConfirmacionArmas = false;

        Debug.Log(
            "Tutorial: selección de armas cerrada. Mostrando panel 5.");

        MostrarPanel(4);
    }

    // =====================================
    // DETECTAR PRIMER ENEMIGO
    // =====================================

    public void NotificarPrimerEnemigo()
    {
        if (tutorialFinalizado || primerEnemigoDetectado)
            return;

        primerEnemigoDetectado = true;

        Debug.Log("Tutorial: apareció el primer enemigo.");

        if (esperandoPrimerEnemigo && !tutorialActivo)
            IniciarEsperaEnemigo();
    }

    // =====================================
    // ESPERAR ANTES DEL PANEL 9
    // =====================================

    private void IniciarEsperaEnemigo()
    {
        if (tutorialFinalizado ||
            tutorialActivo ||
            corrutinaTutorialEnemigo != null)
            return;

        corrutinaTutorialEnemigo =
            StartCoroutine(EsperarEnemigo());
    }

    private IEnumerator EsperarEnemigo()
    {
        yield return new WaitForSeconds(esperaAntesPanel9);

        corrutinaTutorialEnemigo = null;

        if (tutorialFinalizado || tutorialActivo)
            yield break;

        esperandoPrimerEnemigo = false;

        MostrarPanel(8);
    }

    // =====================================
    // BLOQUEAR JUEGO
    // =====================================

    private void BloquearJuego()
    {
        if (!controlesGuardados)
        {
            escalaTiempoAnterior = Time.timeScale;

            movimientoEstabaActivo =
                movimiento != null && movimiento.enabled;

            esquiveEstabaActivo =
                esquive != null && esquive.enabled;

            saltoEstabaActivo =
                salto != null && salto.enabled;

            mouseLookEstabaActivo =
                mouseLook != null && mouseLook.enabled;

            weaponManagerEstabaActivo =
                weaponManager != null && weaponManager.enabled;

            cursorVisibleAnterior = Cursor.visible;
            cursorLockAnterior = Cursor.lockState;

            controlesGuardados = true;
        }

        Time.timeScale = 0f;

        if (movimiento != null)
            movimiento.enabled = false;

        if (esquive != null)
            esquive.enabled = false;

        if (salto != null)
            salto.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;

        if (weaponManager != null)
            weaponManager.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // =====================================
    // RESTAURAR CONTROLES SIN DESPAUSAR
    // =====================================

    private void ReanudarControlesManteniendoPausa()
    {
        if (movimiento != null)
            movimiento.enabled = movimientoEstabaActivo;

        if (esquive != null)
            esquive.enabled = esquiveEstabaActivo;

        if (salto != null)
            salto.enabled = saltoEstabaActivo;

        if (mouseLook != null)
            mouseLook.enabled = mouseLookEstabaActivo;

        if (weaponManager != null)
            weaponManager.enabled = weaponManagerEstabaActivo;

        Cursor.visible = cursorVisibleAnterior;
        Cursor.lockState = cursorLockAnterior;

        // Importante: no cambiar Time.timeScale.
        controlesGuardados = false;
    }

    // =====================================
    // REANUDAR JUEGO
    // =====================================

    private void ReanudarJuego()
    {
        if (movimiento != null)
            movimiento.enabled = movimientoEstabaActivo;

        if (esquive != null)
            esquive.enabled = esquiveEstabaActivo;

        if (salto != null)
            salto.enabled = saltoEstabaActivo;

        if (mouseLook != null)
            mouseLook.enabled = mouseLookEstabaActivo;

        if (weaponManager != null)
            weaponManager.enabled = weaponManagerEstabaActivo;

        Cursor.visible = cursorVisibleAnterior;
        Cursor.lockState = cursorLockAnterior;

        Time.timeScale = escalaTiempoAnterior;

        controlesGuardados = false;
    }

    // =====================================
    // FINALIZAR TUTORIAL
    // =====================================

    private void FinalizarTutorial()
    {
        audioSource.Stop();

        tutorialActivo = false;
        tutorialFinalizado = true;
        esperandoPrimerEnemigo = false;

        for (int i = 0; i < paneles.Length; i++)
        {
            if (paneles[i] != null)
                paneles[i].SetActive(false);
        }

        ReanudarJuego();

        Debug.Log("Tutorial del nivel 1 completado.");
    }

    private void OnDestroy()
    {
        if (audioSource != null)
            audioSource.Stop();

        // No modificar Time.timeScale aquí:
        // otro administrador puede controlar la pausa.
    }
}
