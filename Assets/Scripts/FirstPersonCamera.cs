using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f; // Sensibilidad del rat�n

    private float rotationX = 0f;
    public Transform cameraTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al centro de la pantalla
        Cursor.visible = false; // Oculta el cursor
    }

    void Update()
    {
        // Movimiento de c�mara con el rat�n
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limita la rotaci�n vertical

        cameraTransform.localEulerAngles = new Vector3(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}