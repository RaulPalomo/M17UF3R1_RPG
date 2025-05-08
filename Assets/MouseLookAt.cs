using UnityEngine;

public class MouseLookAt : MonoBehaviour
{
    public Camera mainCamera; // Cámara principal
    public Transform lookAtTarget; // El objeto que está asignado al LookAt de la virtual camera
    public LayerMask raycastLayerMask; // Capa donde quieres que "apunte"
    public float maxRayDistance = 100f;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, raycastLayerMask))
        {
            lookAtTarget.position = hit.point;
        }
        else
        {
            // Si no golpea nada, puedes proyectar en un plano
            Plane plane = new Plane(Vector3.up, Vector3.zero); // Plano horizontal en Y=0
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                lookAtTarget.position = ray.GetPoint(enter);
            }
        }
    }
}

