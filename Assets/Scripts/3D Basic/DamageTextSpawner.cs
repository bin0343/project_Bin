using UnityEngine;
using UnityEngine.UI;

public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner instance;

    public GameObject damageTextPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnDamageText(int damage, Vector3 position, Quaternion rotation, Canvas parentCanvas)
    {
        SpawnDamageText(damage, position, rotation, parentCanvas, false);
    }

    public void SpawnDamageText(int damage, Vector3 position, Quaternion rotation, Canvas parentCanvas, bool isPerfectEvadeBonus)
    {
        if (damageTextPrefab == null || parentCanvas == null)
        {
            Debug.LogError("DamageTextPrefab 또는 ParentCanvas가 null입니다.");
            return;
        }

        GameObject textObject = Instantiate(damageTextPrefab, parentCanvas.transform);

        DamageText dmgText = textObject.GetComponent<DamageText>();
        if (dmgText != null)
        {
            dmgText.SetDamage(damage, isPerfectEvadeBonus);
        }

        textObject.transform.position = position;
        textObject.transform.rotation = rotation;
    }
}
