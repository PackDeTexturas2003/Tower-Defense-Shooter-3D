using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Configuración")]
    public KeyCode teclaDash = KeyCode.LeftControl;
    public float distanciaDash = 6f;
    public float duracionDash = 0.20f;
    public float cooldownDash = 1f;

    private CharacterController controller;

    public bool EstaHaciendoDash { get; private set; }
    public bool PuedeHacerDash { get; private set; } = true;

    private Vector3 direccionDash;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaDash) &&
            PuedeHacerDash &&
            !EstaHaciendoDash)
        {
            ObtenerDireccion();

            StartCoroutine(HacerDash());
        }
    }
    void ObtenerDireccion()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = transform.right * horizontal +
                             transform.forward * vertical;

        if (movimiento.sqrMagnitude > 0.01f)
        {
            direccionDash = movimiento.normalized;
        }
        else
        {
            direccionDash = transform.forward;
        }
    }

    IEnumerator HacerDash()
    {
        PuedeHacerDash = false;
        EstaHaciendoDash = true;

        float velocidadDash = distanciaDash / duracionDash;

        float tiempo = 0f;

        while (tiempo < duracionDash)
        {
            controller.Move(direccionDash * velocidadDash * Time.deltaTime);

            tiempo += Time.deltaTime;

            yield return null;
        }

        EstaHaciendoDash = false;

        yield return new WaitForSeconds(cooldownDash);

        PuedeHacerDash = true;
    }
}