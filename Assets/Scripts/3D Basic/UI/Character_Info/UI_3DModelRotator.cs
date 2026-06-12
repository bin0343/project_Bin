using UnityEngine;
using UnityEngine.EventSystems;

public class UI_3DModelRotator : MonoBehaviour, IDragHandler
{
    [Header("회전시킬 3D 타겟")]
    [Tooltip("Render Texture 카메라 앞에 서 있는 실제 3D 무기/캐릭터 오브젝트를 연결")]
    public Transform target3DObject;

    public float rotationSpeed = 0.5f;

    public void OnDrag(PointerEventData eventData)
    {
        if (target3DObject == null) return;

        float rotX = eventData.delta.x * rotationSpeed;

        float rotY = eventData.delta.y * rotationSpeed;

        target3DObject.Rotate(Vector3.up, -rotX, Space.World);
        target3DObject.Rotate(Vector3.right, rotY, Space.World);
    }
}
