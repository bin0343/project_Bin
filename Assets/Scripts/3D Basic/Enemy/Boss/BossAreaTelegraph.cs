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

    private Transform originalRootParent;
    private Vector3 originalRootLocalPosition;
    private Quaternion originalRootLocalRotation;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponentInChildren<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] " + $"LineRenderer가 없습니다.");

            return;
        }

        originalRootParent = transform.parent;
        originalRootLocalPosition = transform.localPosition;
        originalRootLocalRotation = transform.localRotation;

        if (fillCircle != null)
        {
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

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        center.y += groundOffset;

        transform.position = center;
        transform.rotation = Quaternion.identity;

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
            fillCircle.gameObject.SetActive(true);

            fillCircle.localPosition = Vector3.zero;
            fillCircle.localRotation = Quaternion.identity;

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
        }

        if (originalRootParent != null && transform.parent != originalRootParent)
        {
            transform.SetParent(originalRootParent, false);

            transform.localPosition = originalRootLocalPosition;
            transform.localRotation = originalRootLocalRotation;
        }
    }

    public void OnDisable()
    {
        Hide();
    }
}
