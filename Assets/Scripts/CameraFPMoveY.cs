using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFPMoveY : MonoBehaviour
{
    public Camera cam; // C�mara principal
    private float cameraRotationY = 0f; // acumulador rotaci�n Y
    public float rotationSpeed = 0.1f;  // sensibilidad

    void Update()
    {
       RotateCameraY();
    }

    private void RotateCameraY()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * rotationSpeed;
        cameraRotationY += mouseX;
        cam.transform.localRotation = Quaternion.Euler(0f, cameraRotationY, 0f);
    }
}

