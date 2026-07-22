using UnityEngine;

public class BossDashTelegraph : MonoBehaviour
{
    [Header("경고선")]
    [SerializeField] private LineRenderer lineRenderer;
    [Tooltip("바닥과 겹치는거 막기")]
    [SerializeField, Min(0f)] private float groundOffset = 0.05f;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] " + $"LineRenderer가 없습니다.");

            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        lineRenderer.alignment = LineAlignment.TransformZ;

        Hide();
    }

    public void Show()
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
    }

    public void UpdateLine(Vector3 startPostion, Vector3 endPostion, float width)
    {
        if (lineRenderer == null) return;

        Vector3 direction = endPostion - startPostion;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            Hide();
            return;
        }

        float lineHeight = startPostion.y + groundOffset;

        startPostion.y = lineHeight;
        endPostion.y = lineHeight;

        transform.rotation = Quaternion.LookRotation(Vector3.up, direction.normalized);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.SetPosition(0, startPostion);
        lineRenderer.SetPosition(1, endPostion);

        Show();
    }

    public void Hide()
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
