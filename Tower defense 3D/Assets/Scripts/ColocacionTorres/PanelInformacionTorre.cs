using UnityEngine;
using TMPro;

public class PanelInformacionTorre : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Texto de estadísticas")]
    [SerializeField] private TMP_Text textoInformacion;

    private void Awake()
    {
        if (panel == null)
        {
            Debug.LogWarning(
                "PanelInformacionTorre: Asigna el objeto Panel en el Inspector."
            );
            return;
        }

        panel.SetActive(false);
    }

    public void Mostrar(GameObject prefabTorre, int costo)
    {
        if (prefabTorre == null)
        {
            Ocultar();
            return;
        }

        float danio = 0f;
        float rango = 0f;
        float intervaloDisparo = 0f;
        bool encontrada = false;

        TorretaCortoAlcance corta = prefabTorre.GetComponentInChildren<TorretaCortoAlcance>(true);
        if (corta != null)
        {
            danio = corta.ObtenerDanio();
            rango = corta.ObtenerRango();
            intervaloDisparo = corta.ObtenerTiempoEntreDisparos();
            encontrada = true;
        }
        else
        {
            TorretaLargoAlcance larga = prefabTorre.GetComponentInChildren<TorretaLargoAlcance>(true);
            if (larga != null)
            {
                danio = larga.ObtenerDanio();
                rango = larga.ObtenerRango();
                intervaloDisparo = larga.ObtenerTiempoEntreDisparos();
                encontrada = true;
            }
        }

        if (textoInformacion != null)
        {
            if (encontrada)
            {
                float disparosPorSegundo = intervaloDisparo > 0f ? 1f / intervaloDisparo : 0f;
                textoInformacion.text =
                    "TORRE: " + prefabTorre.name + "\n" +
                    "Costo: $" + costo + "\n" +
                    "Daño: " + danio.ToString("0.##") + "\n" +
                    "Velocidad: " + disparosPorSegundo.ToString("0.##") + " disparos/s\n" +
                    "Radio de detección: " + rango.ToString("0.##") + " m";
            }
            else
            {
                textoInformacion.text =
                    "TORRE: " + prefabTorre.name + "\n" +
                    "Costo: $" + costo + "\n" +
                    "No se encontraron estadísticas de ataque.\n" +
                    "Añade TorretaCortoAlcance o TorretaLargoAlcance al prefab.";
            }
        }

        if (panel != null && !panel.activeSelf)
            panel.SetActive(true);
    }

    public void Ocultar()
    {
        if (panel != null && panel.activeSelf)
            panel.SetActive(false);
    }
}
