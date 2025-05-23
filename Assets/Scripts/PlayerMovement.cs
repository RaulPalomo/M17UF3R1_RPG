using static Player;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float maxHealth = 100f; // salud m�xima del jugador
    public float speed = 5f; // velocidad de movimiento
    private float rotationY = 0f;  // rotaci�n acumulada
    public float rotationSpeed = 0.1f; // velocidad de rotaci�n
    private bool cheering = false; // variable para verificar si el jugador est� celebrando 
    private Animator animator; // referencia al componente Animator
    private Player playerMove;
    private Vector3 movementInput;
    private Rigidbody rb;

    public float jumpCD=1.5f; // tiempo de recarga del salto
    private bool canJump = true; // variable para verificar si el jugador puede saltar
    public float jumpForce = 1.5f; // fuerza de salto
    public bool isGrounded = true; // variable para verificar si el jugador est� en el suelo
    public bool isDead = false; // variable para verificar si el jugador est� muerto    

    public Camera mainCam; // referencia a la c�mara
    public Camera cheerCam; // referencia a la c�mara de celebraci�n

    public CinemachineVirtualCamera vcam; // referencia a la c�mara virtual de cinemachine
    public CinemachineFreeLook vcamFreeLook; // referencia a la c�mara virtual de cinemachine free look

    //bullet
    public GameObject bulletPrefab; // prefab de la bala
    public Transform firePoint; // punto de disparo
    public float bulletSpeed = 20f; // velocidad de la bala
    public Transform targetPoint; // punto objetivo para disparar

    
    public Vector3 lastSpawn;
    private void Awake()
    {
        playerMove = new Player();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al centro de la pantalla
        Cursor.visible = false; // Oculta el cursor
        lastSpawn= GetComponent<Transform>().position;
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
        if (isDead) return; 
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
        if (isGrounded && !cheering &&canJump && !isDead)
        {
            animator.SetTrigger("jump");
            // Convertir movimientoInput (local) a direcci�n mundial
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
        if (isDead) return;
        if (context.performed)
        {
            StartCoroutine(Celebration());
        }
        
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (context.performed)
        {
            animator.SetTrigger("shoot");
            Shoot();
        }
    }

    void Shoot()
    {
        if (isDead) return;

        Vector3 direction = (targetPoint.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = direction * bulletSpeed;
    }
    public IEnumerator Celebration()
    {
        animator.SetTrigger("cheer");
        cheerCam.targetDisplay=0; // Cambia a la c�mara de celebraci�n
        mainCam.enabled = false; // Desactiva la c�mara principal
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
        if (isDead) return;
        context.action.canceled += ctx => animator.SetLayerWeight(animator.GetLayerIndex("AimLayer"), 0f);
        context.action.canceled += ctx => animator.SetBool("aiming", false);
        if (context.performed)
        {
            animator.SetBool("aiming",true);
        }
        animator.SetLayerWeight(animator.GetLayerIndex("AimLayer"), 1f);
        //animator.SetTrigger("aim");
    }

    public void OnRespawn(InputAction.CallbackContext context)
    {
        animator.SetTrigger("jump");
        transform.position = lastSpawn;
        maxHealth = 100f; // reinicia la salud m�xima del jugador
        isDead = false; // reinicia el estado de muerte
    }
    private void RotateTowardsMouse()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * rotationSpeed;  // sensibilidad (ajusta 0.1f si va muy r�pido/lento)
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
    public void TakeDamage(float damage)
    {
        maxHealth -= damage;
        if(maxHealth <= 0&&!isDead)
        {
            //GetComponent<PlayerInput>().enabled = false;
            animator.SetTrigger("die");
            isDead = true;
        }
    }

}
