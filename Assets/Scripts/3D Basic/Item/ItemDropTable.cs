using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropTable : MonoBehaviour
{
    /*private List<DropItem> dropTable;
    private bool isPlayerNearby;

    public void Setup(List<DropItem> table)
    {
        dropTable = table;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UIManager.Instance.ShowMessage("F : 시체확인");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UIManager.Instance.HideMessage();
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            LootUI.Instance.Open(GetDroppedItems());
        }
    }

    List<ItemData> GetDroppedItems()
    {
        List<ItemData> drops = new List<ItemData>();
        foreach (var drop in dropTable)
        {
            if (Random.value <= drop.dropChance)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                for (int i = 0; i < amount; i++)
                {
                    drops.Add(drop.itemData);
                }
            }
        }
        return drops;
    }*/
}
