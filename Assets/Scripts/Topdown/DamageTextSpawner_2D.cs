using UnityEngine;

public class DamageTextSpawner_2D : MonoBehaviour
{
    public GameObject playerDamageTextPrefab;
    public GameObject enemyDamageTextPrefab;

    public void SpawnPlayerDamageText(Vector3 worldPosition, int damageAmount)
    {
        Spawn(worldPosition, damageAmount, playerDamageTextPrefab);
    }

    public void SpawnEnemyDamageText(Vector3 worldPosition, int damageAmount)
    {
        Spawn(worldPosition, damageAmount, enemyDamageTextPrefab);
    }

    private void Spawn(Vector3 worldPosition, int damageAmount, GameObject prefab)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject obj;

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Vector3 spawnPosition = worldPosition + Vector3.up * 0.001f;
            obj = Instantiate(prefab, spawnPosition, Quaternion.identity, canvas.transform);
        }
        else
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            obj = Instantiate(prefab, screenPos, Quaternion.identity, canvas.transform);
        }

        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
        obj.GetComponent<DamageText_2D>().Setup(damageAmount);
    }
}