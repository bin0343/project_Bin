using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlayerAfterImageEffect : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform modelRoot;

    [Header("Meterial")]
    [SerializeField] private Material afterImageMaterial;
    [SerializeField] private Color startColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Spawn")]
    [SerializeField] private int spawnCount = 4;
    [SerializeField] private float spawnInterval = 0.04f;
    [SerializeField] private float lifeTime = 0.35f;

    [Header("Rendering")]
    [SerializeField] private bool useSourceMeshScale = true;
    [SerializeField] private bool disableShadow = true;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Coroutine burstCoroutine;

    private void Awake()
    {
        if (modelRoot == null)
        {
            modelRoot = transform;
        }

        CacheRenderers();
    }

    private void CacheRenderers()
    {
        if (modelRoot == null) return;

        skinnedMeshRenderers = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void PlayBurst()
    {
        if (afterImageMaterial == null)
        {
            Debug.LogWarning("AfterImage Material이 비어 있습니다.");
            return;
        }

        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
        {
            CacheRenderers();
        }

        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
        }

        burstCoroutine = StartCoroutine(SpawnBurstRoutine());
    }

    private IEnumerator SpawnBurstRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnAfterImageOnce();

            if (spawnInterval > 0f)
            {
                yield return new WaitForSecondsRealtime(spawnInterval);
            }
            else
            {
                yield return null;
            }
        }

        burstCoroutine = null;
    }

    private void SpawnAfterImageOnce()
    {
        if (skinnedMeshRenderers == null)
            return;

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            SkinnedMeshRenderer source = skinnedMeshRenderers[i];
            if (source == null || !source.enabled)
                continue;

            Mesh bakedMesh = new Mesh();
            source.BakeMesh(bakedMesh);

            GameObject ghost = new GameObject("AfterImage_" + source.name);
            ghost.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);

            if (useSourceMeshScale)
            {
                ghost.transform.localScale = source.transform.lossyScale;
            }

            MeshFilter meshFilter = ghost.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = bakedMesh;

            MeshRenderer meshRenderer = ghost.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(afterImageMaterial);

            if (disableShadow)
            {
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }

            SetMaterialColor(meshRenderer.sharedMaterial, startColor);

            StartCoroutine(FadeAndDestroy(ghost, meshRenderer.sharedMaterial, bakedMesh));
        }
    }

    private IEnumerator FadeAndDestroy(GameObject ghost, Material material, Mesh mesh)
    {
        float timer = 0f;

        while (timer < lifeTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / lifeTime);

            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            SetMaterialColor(material, color);

            yield return null;
        }

        if (ghost != null)
        {
            Destroy(ghost);
        }

        if (material != null)
        {
            Destroy(material);
        }

        if (mesh != null)
        {
            Destroy(mesh);
        }
    }

    private void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
            return;

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
    }
}
