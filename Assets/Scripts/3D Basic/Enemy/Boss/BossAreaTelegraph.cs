using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAreaTelegraph : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField, Range(12, 128)] private int segmentCount = 64;
    [SerializeField, Min(0.001f)] private float linewidth = 0.12f;
    [SerializeField, Min(0f)] private float groundOffset = 0.05f;

    [Header("내부 채움")]
    [SerializeField] private Transform fillCircle;

    private Transform originalFillParent;

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

        if (fillCircle != null)
        {
            originalFillParent = fillCircle.parent;

            Collider[] fillColliders = fillCircle.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in fillColliders)
            {
                col.enabled = false;
            }
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segmentCount;
        lineRenderer.alignment = LineAlignment.TransformZ;

        transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);

        lineRenderer.startWidth = linewidth;
        lineRenderer.endWidth = linewidth;

        Hide();
    }

    public void Show(Vector3 center, float radius)
    {
        if (lineRenderer == null) return;

        radius = Mathf.Max(radius, 0.01f);

        center.y += groundOffset;

        for (int i = 0; i < segmentCount; i++)
        {
            float normalized = i / (float)segmentCount;
            float angle = normalized * Mathf.PI * 2f;

            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            lineRenderer.SetPosition(i, center + offset);
        }

        lineRenderer.enabled = true;

        if (fillCircle != null)
        {
            if (fillCircle.parent != null)
            {
                fillCircle.SetParent(null, true);
            }

            fillCircle.gameObject.SetActive(true);

            fillCircle.position = center;
            fillCircle.rotation = Quaternion.identity;

            fillCircle.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }
    }

    public void Hide()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        if (fillCircle != null)
        {
            fillCircle.gameObject.SetActive(false);

            if (originalFillParent != null && fillCircle.parent != originalFillParent)
            {
                fillCircle.SetParent(originalFillParent, true);
            }
        }
    }

    public void OnDisable()
    {
        Hide();
    }
}
