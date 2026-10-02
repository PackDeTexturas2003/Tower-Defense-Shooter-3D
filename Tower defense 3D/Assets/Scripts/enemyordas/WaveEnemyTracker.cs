using UnityEngine;

public class WaveEnemyTracker : MonoBehaviour
{
    private WaveManager manager;
    private int indiceOleada;

    private bool configurado;

    public void Configurar(
        WaveManager waveManager,
        int oleada)
    {
        manager = waveManager;
        indiceOleada = oleada;

        configurado = true;
    }

    private void OnDestroy()
    {
        if (!configurado)
            return;

        if (manager == null)
            return;

        manager.RegistrarMuerteEnemigo(
            indiceOleada
        );

        configurado = false;
    }
}