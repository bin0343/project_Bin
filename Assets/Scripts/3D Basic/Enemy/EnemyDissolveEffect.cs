using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDissolveEffect : MonoBehaviour
{
    [Header("Dissolve Setting")]
    [SerializeField] private Shader dissolveShader;
    [SerializeField] private Texture2D dissolveNoiseTexture;

    [Header("Timing")]
    [SerializeField] private float dissolveStartDelay = 0.3f;
    [SerializeField] private float dissolveDuration = 1.2f;

    [Header("Edge")]
    [SerializeField] private Color edgeColor = new Color(1f, 0.55f, 0.15f, 1f);
    [SerializeField, Range(0.01f, 0.2f)] private float edgeWidth = 0.05f;
    [SerializeField, Range(0f, 5f)] private float edgeEmission = 2f;

    [Header("Target")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject excludeObject;

    private Renderer[] renderers;
    private Material[] dissolveMaterials;
    private bool isPlaying = false;

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        List<Renderer> rendererList = new List<Renderer>();

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in allRenderers)
        {
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer))
                continue;

            if (excludeObject != null && r.transform.IsChildOf(excludeObject.transform))
                continue;

            rendererList.Add(r);
        }

        renderers = rendererList.ToArray();
    }

    public void PlayDissolve()
    {
        if (isPlaying)
            return;

        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        isPlaying = true;

        yield return new WaitForSeconds(dissolveStartDelay);

        PrepareDissolveMaterials();

        float timer = 0f;

        while (timer < dissolveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / dissolveDuration;
            float amount = Mathf.Lerp(0f, 1.1f, t);

            SetDissolveAmount(amount);

            yield return null;
        }

        SetDissolveAmount(1.1f);

        if (rootObject != null)
            rootObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void PrepareDissolveMaterials()
    {
        if (dissolveShader == null)
        {
            Debug.LogWarning($"{name} : Dissolve Shader가 비어있습니다.");
            return;
        }

        List<Material> materialList = new List<Material>();

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                    continue;

                mats[i].shader = dissolveShader;

                if (dissolveNoiseTexture != null)
                    mats[i].SetTexture("_NoiseTex", dissolveNoiseTexture);

                mats[i].SetFloat("_DissolveAmount", 0f);
                mats[i].SetColor("_EdgeColor", edgeColor);
                mats[i].SetFloat("_EdgeWidth", edgeWidth);
                mats[i].SetFloat("_EdgeEmission", edgeEmission);

                materialList.Add(mats[i]);
            }

            r.materials = mats;
        }

        dissolveMaterials = materialList.ToArray();
    }

    private void SetDissolveAmount(float amount)
    {
        if (dissolveMaterials == null)
            return;

        foreach (Material mat in dissolveMaterials)
        {
            if (mat == null)
                continue;

            mat.SetFloat("_DissolveAmount", amount);
        }
    }
}