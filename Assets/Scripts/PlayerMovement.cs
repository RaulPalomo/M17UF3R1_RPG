using static Player;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float maxHealth = 100f; // salud máxima del jugador
    public float speed = 5f; // velocidad de movimiento
    private float rotationY = 0f;  // rotación acumulada
    public float rotationSpeed = 0.1f; // velocidad de rotación
    private bool cheering = false; // variable para verificar si el jugador está celebrando 
    private Animator animator; // referencia al componente Animator
    private Player playerMove;
    private Vector3 movementInput;
    private Rigidbody rb;

    public float jumpCD=1.5f; // tiempo de recarga del salto
    private bool canJump = true; // variable para verificar si el jugador puede saltar
    public float jumpForce = 1.5f; // fuerza de salto
    public bool isGrounded = true; // variable para verificar si el jugador está en el suelo


    public Camera mainCam; // referencia a la cámara
    public Camera cheerCam; // referencia a la cámara de celebración

    public CinemachineVirtualCamera vcam; // referencia a la cámara virtual de cinemachine
    public CinemachineFreeLook vcamFreeLook; // referencia a la cámara virtual de cinemachine free look
    private void Awake()
    {
        playerMove = new Player();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al centro de la pantalla
        Cursor.visible = false; // Oculta el cursor
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
        if (isGrounded && !cheering)
        {
            Vector3 move = transform.right * movex + transform.forward * movez;
            transform.position += move.normalized * speed * Time.deltaTime;
            if (movementInput.y > 0)
            {
                animator.SetInteger("dir", 1);
            }
            else if (movementInput.y < 0)
            {
                animator.SetInteger("dir", 3);
            }
            else if (movementInput.x > 0)
            {
                animator.SetInteger("dir", 2);
            }
            else if (movementInput.x < 0)
            {
                animator.SetInteger("dir", 4);
            }
            else
            {
                animator.SetInteger("dir", 0);
            }

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
        if (isGrounded && !cheering &&canJump)
        {
            animator.SetTrigger("jump");
            // Convertir movimientoInput (local) a dirección mundial
            Vector3 moveDir = transform.right * movementInput.x + transform.forward * movementInput.y;
            moveDir.Normalize();

            // Combinar con salto hacia arriba
            Vector3 jumpDir = moveDir + Vector3.up;
            jumpDir.Normalize();

            rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldown());
        }

    }
    private IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpCD);
        canJump = true;
    }
    public void OnCelebrate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(Celebration());
        }
        
    }
    public IEnumerator Celebration()
    {
        animator.SetTrigger("cheer");
        cheerCam.targetDisplay=0; // Cambia a la cámara de celebración
        mainCam.enabled = false; // Desactiva la cámara principal
        cheering = true;
        yield return new WaitForSeconds(2f);
        cheering = false;
        mainCam.enabled = true;
        cheerCam.targetDisplay = 2; 
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
    /*private void RotateTowardsMouse()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Cursor.visible = false;
        transform.rotation = Quaternion.LookRotation(forward);
    }*/
    public void OnAim(InputAction.CallbackContext context)
    {
        context.action.canceled += ctx => animator.SetLayerWeight(animator.GetLayerIndex("AimLayer"), 0f);
        animator.SetLayerWeight(animator.GetLayerIndex("AimLayer"), 1f);
        animator.SetTrigger("aim");
    }

    private void RotateTowardsMouse()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * rotationSpeed;  // sensibilidad (ajusta 0.1f si va muy rápido/lento)
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
    public void TakeDamage(float damage)
    {
        maxHealth -= damage;
    }

}
