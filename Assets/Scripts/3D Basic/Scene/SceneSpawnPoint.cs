using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [Header("스폰 위치 식별자")]
    [SerializeField] private string spawnID;

    public string SpawnID
    {
        get { return spawnID; }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position + Vector3.up * 0.5f,
            0.4f
        );

        Gizmos.DrawRay(
            transform.position + Vector3.up * 0.5f,
            transform.forward * 1.5f
        );
    }
#endif
}
