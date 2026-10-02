using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 5f;
    [SerializeField] private float velocidadCorrer = 8f;

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 2f;
    [SerializeField] private float gravedad = -20f;

    private CharacterController controller;

    private Vector3 velocidadVertical;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        Movimiento();

        AplicarGravedad();

        Salto();
    }

    void Movimiento()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direccion = transform.right * horizontal +
                            transform.forward * vertical;

        direccion.Normalize();

        float velocidadActual = Input.GetKey(KeyCode.LeftShift)
            ? velocidadCorrer
            : velocidadCaminar;

        controller.Move(direccion * velocidadActual * Time.deltaTime);
    }

    void AplicarGravedad()
    {
        if (controller.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f;
        }

        velocidadVertical.y += gravedad * Time.deltaTime;

        controller.Move(velocidadVertical * Time.deltaTime);
    }

    void Salto()
    {
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocidadVertical.y =
                Mathf.Sqrt(alturaSalto * -2f * gravedad);
        }
    }
}