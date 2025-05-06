using static Player;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5f; // velocidad de movimiento
    private Player playerMove;
    private Vector3 movementInput;
    private Rigidbody rb;
    public float jumpForce = 1.5f; // fuerza de salto
    public bool isGrounded = true; // variable para verificar si el jugador está en el suelo
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
            Vector3 dir= Vector3.up;
            dir.x = movementInput.x;
            dir.z = movementInput.y;
            dir.Normalize();
            rb.AddForce(dir * jumpForce, ForceMode.Impulse);
        }
        
    }

}
