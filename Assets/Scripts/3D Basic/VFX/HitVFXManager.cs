using UnityEngine;

public class HitVFXManager : MonoBehaviour
{
    public static HitVFXManager instance;

    [Header("Enemy Hit VFX")]
    [SerializeField] private GameObject normalEnemyHitPrefab;
    [SerializeField] private GameObject criticalEnemyHitPrefab;

    [Header("Player Hit VFX")]
    [SerializeField] private GameObject playerHitPrefab;

    [Header("Spawn Setting")]
    [SerializeField] private float defaultDestroyTime = 1.0f;
    [SerializeField] private Vector3 rotationOffsetEuler;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void PlayEnemyHit(Vector3 position, Vector3 hitDirection, bool isCriticalHit)
    {
        GameObject prefab = isCriticalHit ? criticalEnemyHitPrefab : normalEnemyHitPrefab;

        SpawnHitVFX(prefab, position, hitDirection);
    }

    public void PlayPlayerHit(Vector3 position, Vector3 hitDirection)
    {
        SpawnHitVFX(playerHitPrefab, position, hitDirection);
    }

    private void SpawnHitVFX(GameObject prefab, Vector3 position, Vector3 hitDirection)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.identity;

        if (hitDirection.sqrMagnitude > 0.001f)
        {
            rotation = Quaternion.LookRotation(hitDirection.normalized);
        }

        rotation *= Quaternion.Euler(rotationOffsetEuler);

        GameObject vfx = Instantiate(prefab, position, rotation);

        if (defaultDestroyTime > 0f)
        {
            Destroy(vfx, defaultDestroyTime);
        }
    }
}
