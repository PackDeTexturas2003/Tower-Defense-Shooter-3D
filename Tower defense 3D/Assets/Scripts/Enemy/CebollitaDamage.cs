
using UnityEngine;

public class CebollitaDamage : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private float dano = 10f;
    [SerializeField] private float intervaloDanio = 1f;

    [Header("Detección del jugador")]
    [SerializeField] private float radioAtaque = 2f;
    [SerializeField] private string tagJugador = "Player";

    private Transform jugador;
    private float siguienteDanio;

    private void Update()
    {
        BuscarJugador();

        if (jugador == null)
            return;

        float distancia = Vector3.Distance(
            transform.position,
            jugador.position
        );

        if (distancia <= radioAtaque)
        {
            if (Time.time >= siguienteDanio)
            {
                HacerDanio();
                siguienteDanio = Time.time + intervaloDanio;
            }
        }
    }

    private void BuscarJugador()
    {
        if (jugador != null)
            return;

        GameObject objetoJugador =
            GameObject.FindGameObjectWithTag(tagJugador);

        if (objetoJugador != null)
            jugador = objetoJugador.transform;
    }

    private void HacerDanio()
    {
        PlayerHealth vida =
            jugador.GetComponentInParent<PlayerHealth>();

        if (vida == null)
            vida = jugador.GetComponent<PlayerHealth>();

        if (vida != null)
        {
            vida.RecibirDanio(dano);
            Debug.Log("La cebollita hizo " + dano + " de daño.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
    }
}
