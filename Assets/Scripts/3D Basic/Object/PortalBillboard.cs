using UnityEngine;

public class PortalBillboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            Vector3 targetPosition = mainCameraTransform.position;
            targetPosition.y = transform.position.y;

            transform.LookAt(targetPosition);
        }
    }
}
