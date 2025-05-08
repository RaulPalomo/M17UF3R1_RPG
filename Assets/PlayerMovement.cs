using static Player;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5f; // velocidad de movimiento
    private Player playerMove;
    private Vector3 movementInput;
    private Rigidbody rb;
    public float jumpForce = 1.5f; // fuerza de salto
    public bool isGrounded = true; // variable para verificar si el jugador está en el suelo
    public Camera cam; // referencia a la cámara
    public CinemachineVirtualCamera vcam; // referencia a la cámara virtual de cinemachine
    public CinemachineFreeLook vcamFreeLook; // referencia a la cámara virtual de cinemachine free look
    private void Awake()
    {
        playerMove = new Player();
        rb = GetComponent<Rigidbody>();
        
    }
    void OnEnable()
    {
        playerMove.Enable();
    }

    void OnDisable()
    {
        playerMove.Disable();
    }
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f);
        // movimiento con wasd
        float movex = movementInput.x; // a y d
        float movez = movementInput.y;   // w y s
        if (isGrounded)
        {
            Vector3 move = transform.right * movex + transform.forward * movez;
            transform.position += move.normalized * speed * Time.deltaTime;
        }
        RotateTowardsMouse();
    }
    public void OnMove(InputAction.CallbackContext context)
    {

        movementInput = context.ReadValue<Vector3>();

    }
    public void OnJump(InputAction.CallbackContext context)
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f);
        if (isGrounded)
        {
            // Convertir movimientoInput (local) a dirección mundial
            Vector3 moveDir = transform.right * movementInput.x + transform.forward * movementInput.y;
            moveDir.Normalize();

            // Combinar con salto hacia arriba
            Vector3 jumpDir = moveDir + Vector3.up;
            jumpDir.Normalize();

            rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
        }

    }
    public void OnChangeCamera(InputAction.CallbackContext context)
    {
        if(vcam.Priority>vcamFreeLook.Priority)
        {
            vcam.Priority = 0;
            vcamFreeLook.Priority = 1;
        }
        else
        {
            vcam.Priority = 1;
            vcamFreeLook.Priority = 0;
        }
    }
    private void RotateTowardsMouse()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Cursor.visible = false;
        transform.rotation = Quaternion.LookRotation(forward);
    }

}
