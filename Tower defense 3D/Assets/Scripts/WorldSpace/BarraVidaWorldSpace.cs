
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraVidaWorldSpace : MonoBehaviour
{
    [Header("Entidad con vida")]
    [SerializeField] private Health entidadVida;

    [Header("Elementos de la interfaz")]
    [SerializeField] private Image barraVida;
    [SerializeField] private TMP_Text textoVida;

    [Header("Jugador")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private Transform camaraJugador;

    [Header("Configuración")]
    [SerializeField] private bool mostrarVidaNumerica = true;

    [Header("Comportamiento especial del objetivo")]
    [SerializeField] private bool esObjetivo = false;

    [Header("HUD general")]
    [SerializeField] private Canvas canvasHUD;
    [SerializeField] private RectTransform posicionHUDIzquierda;
    [SerializeField] private RectTransform posicionHUDDerecha;

    [Header("Tamaño del HUD")]
    [SerializeField] private Vector3 escalaHUD = Vector3.one;

    [Header("Detección de visión")]
    [SerializeField] private bool detectarObstaculos = true;
    [SerializeField] private LayerMask capasObstaculos = ~0;
    [SerializeField] private float margenVision = 0.02f;

    [Header("Transición")]
    [SerializeField] private float velocidadTransicion = 8f;

    private RectTransform rectTransformCanvas;
    private RectTransform rectTransformHUD;

    private Canvas canvasWorldSpace;
    private Camera componenteCamaraJugador;

    private Transform padreOriginal;

    private Vector3 posicionOriginalLocal;
    private Quaternion rotacionOriginalLocal;
    private Vector3 escalaOriginalLocal;

    private Vector2 anclaMinOriginal;
    private Vector2 anclaMaxOriginal;
    private Vector2 pivoteOriginal;

    private RenderMode modoOriginal;
    private Camera camaraCanvasOriginal;
    private float distanciaCanvasOriginal;

    private int indiceOriginal;
    private bool overrideSortingOriginal;
    private int ordenSortingOriginal;

    private float distanciaOriginalCamara;

    private bool enHUD;
    private bool canvasEnHUD;
    private bool transicionandoAlObjetivo;

    private Vector2 velocidadPosicionHUD;
    private Vector3 velocidadEscala;
    private Vector3 velocidadPosicionMundo;

    private enum LadoHUD
    {
        Izquierda,
        Derecha
    }

    private LadoHUD ultimoLadoHUD = LadoHUD.Derecha;

    private void Awake()
    {
        rectTransformCanvas = GetComponent<RectTransform>();
        canvasWorldSpace = GetComponent<Canvas>();

        BuscarEntidadVida();
        BuscarCamaraJugador();

        if (entidadVida == null)
        {
            Debug.LogError(
                $"No se encontró un componente Health en los padres ni en los hijos de {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        if (barraVida == null)
        {
            Debug.LogError(
                $"No se asignó la Image de la barra de vida en {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        if (rectTransformCanvas == null)
        {
            Debug.LogError(
                $"No se encontró un RectTransform en {gameObject.name}.",
                this
            );

            enabled = false;
            return;
        }

        padreOriginal = transform.parent;
        indiceOriginal = transform.GetSiblingIndex();

        posicionOriginalLocal = rectTransformCanvas.localPosition;
        rotacionOriginalLocal = rectTransformCanvas.localRotation;
        escalaOriginalLocal = rectTransformCanvas.localScale;

        anclaMinOriginal = rectTransformCanvas.anchorMin;
        anclaMaxOriginal = rectTransformCanvas.anchorMax;
        pivoteOriginal = rectTransformCanvas.pivot;

        if (canvasWorldSpace != null)
        {
            modoOriginal = canvasWorldSpace.renderMode;
            camaraCanvasOriginal = canvasWorldSpace.worldCamera;
            distanciaCanvasOriginal = canvasWorldSpace.planeDistance;

            overrideSortingOriginal = canvasWorldSpace.overrideSorting;
            ordenSortingOriginal = canvasWorldSpace.sortingOrder;
        }

        if (componenteCamaraJugador != null)
        {
            distanciaOriginalCamara = Vector3.Distance(
                componenteCamaraJugador.transform.position,
                rectTransformCanvas.position
            );
        }

        distanciaOriginalCamara = Mathf.Max(
            distanciaOriginalCamara,
            0.01f
        );

        if (esObjetivo)
        {
            rectTransformHUD = canvasHUD != null
                ? canvasHUD.transform as RectTransform
                : null;

            if (canvasHUD == null ||
                canvasWorldSpace == null ||
                posicionHUDIzquierda == null ||
                posicionHUDDerecha == null ||
                rectTransformHUD == null ||
                canvasHUD.renderMode != RenderMode.ScreenSpaceOverlay ||
                componenteCamaraJugador == null)
            {
                Debug.LogError(
                    "Faltan referencias necesarias para el comportamiento especial del objetivo. Se mantendrá el comportamiento original.",
                    this
                );

                esObjetivo = false;
            }
        }

        barraVida.type = Image.Type.Filled;
        barraVida.fillMethod = Image.FillMethod.Horizontal;
        barraVida.fillOrigin = (int)Image.OriginHorizontal.Left;

        ActualizarInterfaz();
    }

    private void LateUpdate()
    {
        if (entidadVida != null)
        {
            ActualizarInterfaz();
        }

        if (camaraJugador == null)
        {
            BuscarCamaraJugador();
        }

        if (esObjetivo && componenteCamaraJugador != null)
        {
            ActualizarComportamientoObjetivo();

            if (enHUD != canvasEnHUD)
            {
                CambiarModoHUD(enHUD);
            }

            if (canvasEnHUD)
            {
                ActualizarPosicionHUD();
            }
            else if (transicionandoAlObjetivo)
            {
                ActualizarRegresoObjetivo();
            }
            else
            {
                OrientarCanvas();
            }
        }
        else
        {
            OrientarCanvas();
        }
    }

    private void BuscarEntidadVida()
    {
        if (entidadVida != null)
            return;

        entidadVida = GetComponentInParent<Health>();

        if (entidadVida == null)
        {
            entidadVida = GetComponentInChildren<Health>();
        }
    }

    private void BuscarCamaraJugador()
    {
        if (camaraJugador == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag(tagJugador);

            if (jugador == null)
            {
                Debug.LogWarning(
                    $"No se encontró ningún objeto con el tag '{tagJugador}'.",
                    this
                );

                return;
            }

            Camera camara = jugador.GetComponentInChildren<Camera>();

            if (camara != null)
            {
                camaraJugador = camara.transform;
            }
            else
            {
                camaraJugador = jugador.transform;
            }
        }

        componenteCamaraJugador = camaraJugador.GetComponent<Camera>();

        if (componenteCamaraJugador == null)
        {
            componenteCamaraJugador =
                camaraJugador.GetComponentInChildren<Camera>();
        }
    }

    private void OrientarCanvas()
    {
        if (camaraJugador == null || rectTransformCanvas == null)
            return;

        rectTransformCanvas.rotation = camaraJugador.rotation;
    }

    private void ActualizarInterfaz()
    {
        float vidaMaxima = entidadVida.VidaMaxima;
        float vidaActual = entidadVida.VidaActual;

        barraVida.fillAmount = vidaMaxima > 0f
            ? Mathf.Clamp01(vidaActual / vidaMaxima)
            : 0f;

        if (textoVida != null)
        {
            textoVida.gameObject.SetActive(mostrarVidaNumerica);

            if (mostrarVidaNumerica)
            {
                textoVida.text =
                    $"{Mathf.CeilToInt(vidaActual)} / {Mathf.CeilToInt(vidaMaxima)}";
            }
        }
    }

    private void ActualizarComportamientoObjetivo()
    {
        Vector3 posicionObjetivo = entidadVida.transform.position;

        Vector3 posicionViewport =
            componenteCamaraJugador.WorldToViewportPoint(posicionObjetivo);

        bool delanteCamara = posicionViewport.z > 0f;

        bool fueraIzquierda = posicionViewport.x < -margenVision;
        bool fueraDerecha = posicionViewport.x > 1f + margenVision;

        bool dentroHorizontal =
            !fueraIzquierda && !fueraDerecha;

        bool dentroVertical =
            posicionViewport.y >= -margenVision &&
            posicionViewport.y <= 1f + margenVision;

        bool dentroPantalla =
            delanteCamara &&
            dentroHorizontal &&
            dentroVertical;

        bool bloqueado = false;

        if (delanteCamara && dentroHorizontal && detectarObstaculos)
        {
            bloqueado = EstaBloqueado(posicionObjetivo);
        }

        bool objetivoVisible = dentroPantalla && !bloqueado;

        if (objetivoVisible)
        {
            enHUD = false;
            return;
        }

        // Si sale por arriba o abajo, mantener la barra sobre el objetivo.
        if (delanteCamara && dentroHorizontal && !bloqueado && !dentroVertical)
        {
            enHUD = false;
            return;
        }

        // Determinar el lado solamente al entrar al HUD.
        if (!enHUD)
        {
            if (fueraIzquierda)
            {
                ultimoLadoHUD = LadoHUD.Izquierda;
            }
            else if (fueraDerecha)
            {
                ultimoLadoHUD = LadoHUD.Derecha;
            }
            else if (!delanteCamara)
            {
                DeterminarLadoHUD(posicionViewport, false);
            }
        }

        enHUD = true;
    }

    private bool EstaBloqueado(Vector3 posicionObjetivo)
    {
        Vector3 origen = componenteCamaraJugador.transform.position;

        Vector3 direccion = posicionObjetivo - origen;

        float distancia = direccion.magnitude;

        if (Physics.Raycast(
            origen,
            direccion.normalized,
            out RaycastHit impacto,
            distancia,
            capasObstaculos,
            QueryTriggerInteraction.Ignore))
        {
            bool perteneceAlObjetivo =
                impacto.transform == entidadVida.transform ||
                impacto.transform.IsChildOf(entidadVida.transform) ||
                entidadVida.transform.IsChildOf(impacto.transform);

            return !perteneceAlObjetivo;
        }

        return false;
    }

    private void DeterminarLadoHUD(
        Vector3 posicionViewport,
        bool delanteCamara)
    {
        if (!delanteCamara)
        {
            Vector3 direccion =
                entidadVida.transform.position -
                componenteCamaraJugador.transform.position;

            float lado = Vector3.Dot(
                direccion,
                componenteCamaraJugador.transform.right
            );

            ultimoLadoHUD = lado < 0f
                ? LadoHUD.Izquierda
                : LadoHUD.Derecha;

            return;
        }

        if (posicionViewport.x < -margenVision)
        {
            ultimoLadoHUD = LadoHUD.Izquierda;
        }
        else if (posicionViewport.x > 1f + margenVision)
        {
            ultimoLadoHUD = LadoHUD.Derecha;
        }
        else if (posicionViewport.y < -margenVision ||
                 posicionViewport.y > 1f + margenVision)
        {
            return;
        }
        else
        {
            ultimoLadoHUD = posicionViewport.x < 0.5f
                ? LadoHUD.Izquierda
                : LadoHUD.Derecha;
        }
    }

    private void CambiarModoHUD(bool activar)
    {
        if (activar)
        {
            Vector3 posicionPantalla =
                componenteCamaraJugador.WorldToScreenPoint(
                    rectTransformCanvas.position
                );

            if (posicionPantalla.z <= 0f)
            {
                RectTransform destino =
                    ultimoLadoHUD == LadoHUD.Izquierda
                        ? posicionHUDIzquierda
                        : posicionHUDDerecha;

                posicionPantalla =
                    RectTransformUtility.WorldToScreenPoint(
                        null,
                        destino.position
                    );
            }

            // Guardar el tamaño visual antes de cambiar el Canvas.
            Vector3 escalaVisualOriginal = rectTransformCanvas.lossyScale;

            canvasWorldSpace.renderMode = RenderMode.ScreenSpaceOverlay;

            rectTransformCanvas.SetParent(
                canvasHUD.transform,
                false
            );

            // Recuperar el tamaño visual después de cambiar el padre.
            Vector3 escalaPadre = canvasHUD.transform.lossyScale;

            rectTransformCanvas.localScale = new Vector3(
                Mathf.Abs(escalaPadre.x) > 0.0001f
                    ? escalaVisualOriginal.x / escalaPadre.x
                    : escalaVisualOriginal.x,

                Mathf.Abs(escalaPadre.y) > 0.0001f
                    ? escalaVisualOriginal.y / escalaPadre.y
                    : escalaVisualOriginal.y,

                Mathf.Abs(escalaPadre.z) > 0.0001f
                    ? escalaVisualOriginal.z / escalaPadre.z
                    : escalaVisualOriginal.z
            );

            rectTransformCanvas.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransformCanvas.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransformCanvas.pivot = new Vector2(0.5f, 0.5f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransformHUD,
                new Vector2(posicionPantalla.x, posicionPantalla.y),
                null,
                out Vector2 posicionLocal
            );

            rectTransformCanvas.anchoredPosition = posicionLocal;
            rectTransformCanvas.localRotation = Quaternion.identity;

            canvasEnHUD = true;
            transicionandoAlObjetivo = false;

            velocidadPosicionHUD = Vector2.zero;
            velocidadEscala = Vector3.zero;
        }
        else
        {
            Vector3 posicionPantalla =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    rectTransformCanvas.position
                );

            rectTransformCanvas.SetParent(
                padreOriginal,
                false
            );

            rectTransformCanvas.anchorMin = anclaMinOriginal;
            rectTransformCanvas.anchorMax = anclaMaxOriginal;
            rectTransformCanvas.pivot = pivoteOriginal;

            canvasWorldSpace.renderMode = modoOriginal;
            canvasWorldSpace.worldCamera = camaraCanvasOriginal;
            canvasWorldSpace.planeDistance = distanciaCanvasOriginal;

            canvasWorldSpace.overrideSorting = overrideSortingOriginal;
            canvasWorldSpace.sortingOrder = ordenSortingOriginal;

            rectTransformCanvas.SetSiblingIndex(indiceOriginal);

            // Recuperar inmediatamente la escala original.
            rectTransformCanvas.localScale = escalaOriginalLocal;

            rectTransformCanvas.position =
                componenteCamaraJugador.ScreenToWorldPoint(
                    new Vector3(
                        posicionPantalla.x,
                        posicionPantalla.y,
                        distanciaOriginalCamara
                    )
                );

            rectTransformCanvas.rotation =
                componenteCamaraJugador.transform.rotation;

            canvasEnHUD = false;
            transicionandoAlObjetivo = true;

            velocidadPosicionMundo = Vector3.zero;
            velocidadEscala = Vector3.zero;
        }
    }

    private void ActualizarPosicionHUD()
    {
        RectTransform destino =
            ultimoLadoHUD == LadoHUD.Izquierda
                ? posicionHUDIzquierda
                : posicionHUDDerecha;

        Vector3 posicionPantalla =
            RectTransformUtility.WorldToScreenPoint(
                null,
                destino.position
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransformHUD,
            new Vector2(posicionPantalla.x, posicionPantalla.y),
            null,
            out Vector2 posicionLocal
        );

        float tiempoSuavizado = 1f /
            Mathf.Max(velocidadTransicion, 0.01f);

        rectTransformCanvas.anchoredPosition =
            Vector2.SmoothDamp(
                rectTransformCanvas.anchoredPosition,
                posicionLocal,
                ref velocidadPosicionHUD,
                tiempoSuavizado
            );

        rectTransformCanvas.localScale =
            Vector3.SmoothDamp(
                rectTransformCanvas.localScale,
                escalaHUD,
                ref velocidadEscala,
                tiempoSuavizado
            );
    }

    private void ActualizarRegresoObjetivo()
    {
        Vector3 posicionDestino = padreOriginal != null
            ? padreOriginal.TransformPoint(posicionOriginalLocal)
            : posicionOriginalLocal;

        float tiempoSuavizado = 1f /
            Mathf.Max(velocidadTransicion, 0.01f);

        rectTransformCanvas.position =
            Vector3.SmoothDamp(
                rectTransformCanvas.position,
                posicionDestino,
                ref velocidadPosicionMundo,
                tiempoSuavizado
            );

        rectTransformCanvas.rotation =
            Quaternion.Slerp(
                rectTransformCanvas.rotation,
                componenteCamaraJugador.transform.rotation,
                1f - Mathf.Exp(-velocidadTransicion * Time.deltaTime)
            );

        // Mantener la escala original durante todo el regreso.
        rectTransformCanvas.localScale = escalaOriginalLocal;

        bool posicionAlcanzada =
            Vector3.Distance(
                rectTransformCanvas.position,
                posicionDestino
            ) < 0.01f;

        if (posicionAlcanzada)
        {
            rectTransformCanvas.localPosition = posicionOriginalLocal;
            rectTransformCanvas.localScale = escalaOriginalLocal;

            transicionandoAlObjetivo = false;
        }
    }
}