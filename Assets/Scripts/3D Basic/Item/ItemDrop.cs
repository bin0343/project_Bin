using System.Collections.Generic;
using UnityEngine;

public enum LootMethod
{
    DropOnGround,           //바닥에 떨어짐(잡몹)
    InteractionWindow       //시체 상호작용(보스)
}

[System.Serializable]
public class LootItem
{
    public Item_Base itemData;
    public int minAmount = 1;
    public int maxAmount = 1;
    [Range(0f, 100f)]
    public float dropChance = 100f;
}

public class ItemDrop : MonoBehaviour
{
    [Header("루팅 방식")]
    public LootMethod lootMethod = LootMethod.DropOnGround;

    [Header("드랍 테이블")]
    public List<LootItem> dropTable = new List<LootItem>();

    // 생성된 아이템을 보관할 리스트(interaction용)
    private List<Item_Base> generatedLoot = new List<Item_Base>();
    private bool isLootGenerated = false;

    [Header("필드 아이템 프리팹")]
    public GameObject fieldItemPrefab;
    public Transform dropSpawnPoint;

    /// <summary>
    /// 몬스터가 죽을 때 '한 번만' 호출되어 드랍될 아이템을 결정하고 보관합니다.
    /// </summary>
    public void GenerateLoot()
    {
        if (isLootGenerated) return; // 중복 생성을 방지

        List<Item_Base> tempLoot = new List<Item_Base>();

        foreach (var loot in dropTable)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= loot.dropChance)
            {
                int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    tempLoot.Add(loot.itemData);
                }
            }
        }

        if (lootMethod == LootMethod.DropOnGround)
        {
            SpawnFieldItems(tempLoot);
            generatedLoot.Clear();
        }
        else if (lootMethod == LootMethod.InteractionWindow)
        {
            generatedLoot = tempLoot;
        }

        isLootGenerated = true;
        /*generatedLoot = new List<Item_Base>();

        foreach (var loot in dropTable)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= loot.dropChance)
            {
                int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    generatedLoot.Add(loot.itemData);
                }
            }
        }
        isLootGenerated = true;
        Debug.Log(generatedLoot.Count + "개의 아이템 드랍 생성 완료.");*/
    }

    private void SpawnFieldItems(List<Item_Base> itemsToDrop)
    {
        Vector3 spawnPos = dropSpawnPoint != null ? dropSpawnPoint.position : transform.position;

        foreach (var item in itemsToDrop)
        {
            if (fieldItemPrefab == null) continue;

            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            GameObject go = Instantiate(fieldItemPrefab, spawnPos + randomOffset, Quaternion.identity);

            FieldItem fieldItem = go.GetComponent<FieldItem>();
            if (fieldItem != null)
            {
                fieldItem.Setup(item);
            }
        }
    }


    /// <summary>
    /// 다른 스크립트가 보관된 아이템 목록을 요청할 때 사용합니다.
    /// </summary>
    public List<Item_Base> GetLoot()
    {
        // 아직 아이템이 생성되지 않았다면 빈 리스트를 반환
        if (!isLootGenerated)
        {
            return new List<Item_Base>();
        }
        return generatedLoot;
    }

    public void RemoveLootedItem(Item_Base item)
    {
        /*if (generatedLoot != null)
        {
            generatedLoot.Remove(item);
        }*/

        if (generatedLoot.Contains(item))
        {
            generatedLoot.Remove(item);
        }
    }

    public bool HasLoot()
    {
        return generatedLoot != null && generatedLoot.Count > 0;
    }
}
