using System.Collections.Generic;
using UnityEngine;

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
    [Header("드랍 테이블")]
    public List<LootItem> dropTable = new List<LootItem>();

    // 생성된 아이템을 보관할 리스트
    private List<Item_Base> generatedLoot;
    private bool isLootGenerated = false;

    /// <summary>
    /// 몬스터가 죽을 때 '한 번만' 호출되어 드랍될 아이템을 결정하고 보관합니다.
    /// </summary>
    public void GenerateLoot()
    {
        if (isLootGenerated) return; // 중복 생성을 방지

        generatedLoot = new List<Item_Base>();

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
        Debug.Log(generatedLoot.Count + "개의 아이템 드랍 생성 완료.");
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
        if (generatedLoot != null)
        {
            generatedLoot.Remove(item);
        }
    }
}
