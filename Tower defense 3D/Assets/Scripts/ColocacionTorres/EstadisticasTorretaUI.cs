using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EstadisticasTorretaUI : MonoBehaviour
{
    [System.Serializable]
    public class InformacionTorreta
    {
        [Header("Torreta")]
        public GameObject prefabTorreta;

        [Header("Imagen informativa")]
        public Image imagenInformacion;

        [Header("Textos de información")]
        public TMP_Text textoRango;
        public TMP_Text textoDaño;
        public TMP_Text textoTiempoEntreDisparos;
        public TMP_Text textoCosto;
    }

    [Header("Información de cada torreta")]
    [SerializeField] private InformacionTorreta[] informacionTorretas;

    [Header("Sistema de colocación")]
    [SerializeField] private ColocacionTorretas colocacionTorretas;

    private void Awake()
    {
        ActualizarImagenes();
    }

    public void ActualizarImagenes()
    {
        if (informacionTorretas == null)
            return;

        foreach (InformacionTorreta informacion in informacionTorretas)
        {
            if (informacion == null || informacion.imagenInformacion == null)
                continue;

            informacion.imagenInformacion.gameObject.SetActive(
                informacion.prefabTorreta != null
            );

            if (informacion.prefabTorreta == null)
                continue;

            ActualizarTextos(informacion);
        }
    }

    private void ActualizarTextos(InformacionTorreta informacion)
    {
        float rango;
        float daño;
        float tiempo;

        TorretaLargoAlcance torreLarga =
            informacion.prefabTorreta.GetComponent<TorretaLargoAlcance>();

        TorretaCortoAlcance torreCorta =
            informacion.prefabTorreta.GetComponent<TorretaCortoAlcance>();

        if (torreLarga != null)
        {
            rango = torreLarga.Rango;
            daño = torreLarga.Daño;
            tiempo = torreLarga.TiempoEntreDisparos;
        }
        else if (torreCorta != null)
        {
            rango = torreCorta.Rango;
            daño = torreCorta.Daño;
            tiempo = torreCorta.TiempoEntreDisparos;
        }
        else
        {
            Debug.LogWarning(
                "El prefab " + informacion.prefabTorreta.name +
                " no tiene un componente de torreta compatible."
            );

            return;
        }

        if (informacion.textoRango != null)
        {
            informacion.textoRango.text =
                "Rango de alcance: " + rango.ToString("0.##");
        }

        if (informacion.textoDaño != null)
        {
            informacion.textoDaño.text =
                "Daño: " + daño.ToString("0.##");
        }

        if (informacion.textoTiempoEntreDisparos != null)
        {
            informacion.textoTiempoEntreDisparos.text =
                "Tiempo entre disparos: " +
                tiempo.ToString("0.##") + " s";
        }

        if (informacion.textoCosto != null)
        {
            if (colocacionTorretas != null)
            {
                int costo = colocacionTorretas.ObtenerPrecioTorreParaUI(
                    informacion.prefabTorreta
                );

                informacion.textoCosto.text = "Costo: " + costo;
            }
            else
            {
                Debug.LogWarning(
                    "No se asignó ColocacionTorretas en EstadisticasTorretaUI."
                );
            }
        }
    }

    public void MostrarEstadisticas(GameObject prefabTorreta)
    {
        ActualizarImagenes();
    }
}