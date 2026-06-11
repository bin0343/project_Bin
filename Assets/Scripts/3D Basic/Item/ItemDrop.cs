using System.Collections.Generic;
using UnityEngine;

public enum LootMethod
{
    AutoAcquire,         // 잡몹: 처치 시 인벤토리로 자동 획득
    BossRewardObject     // 보스: 처치 시 보상 상자/꽃 생성 (F 상호작용)
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
    public LootMethod lootMethod = LootMethod.AutoAcquire;

    [Header("드랍 테이블")]
    public List<LootItem> dropTable = new List<LootItem>();

    [Header("보스 보상 설정 (BossRewardObject 선택 시)")]
    public GameObject bossRewardPrefab; // 생성될 보상 오브젝트 (꽃, 보물상자 등)
    public Transform dropSpawnPoint;    // 생성 위치 (비워두면 몬스터 위치)

    private bool isLootGenerated = false;

    public void GenerateLoot()
    {
        if (isLootGenerated) return;

        List<Item_Base> generatedLoot = new List<Item_Base>();

        // 1. 드랍 확률에 따라 아이템 리스트 생성
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

        // 2. 루팅 방식에 따른 분기 처리
        if (lootMethod == LootMethod.AutoAcquire)
        {
            AutoAcquireItems(generatedLoot);
        }
        else if (lootMethod == LootMethod.BossRewardObject)
        {
            SpawnBossRewardObject(generatedLoot);
        }

        isLootGenerated = true;
    }

    private void AutoAcquireItems(List<Item_Base> lootList)
    {
        if (lootList.Count == 0) return;

        foreach (var item in lootList)
        {
            if (Player_Inventory.instance != null)
            {
                Player_Inventory.instance.AddItem(item, 1);
            }
        }

        Debug.Log($"잡몹 처치! {lootList.Count}개의 아이템이 인벤토리로 자동 획득되었습니다.");

        // 화면 중앙에 시스템 메시지 띄우기
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.ShowMessage($"아이템 {lootList.Count}개 획득!");
        }
    }

    private void SpawnBossRewardObject(List<Item_Base> lootList)
    {
        if (bossRewardPrefab == null)
        {
            Debug.LogError("보상 오브젝트 프리팹이 설정되지 않았습니다!");
            return;
        }

        Vector3 spawnPos = dropSpawnPoint != null ? dropSpawnPoint.position : transform.position;

        // 보상 오브젝트 생성 (바닥에 살짝 띄우기)
        GameObject rewardObj = Instantiate(bossRewardPrefab, spawnPos + Vector3.up * 0.5f, Quaternion.identity);

        // 보상 오브젝트 안에 있는 스크립트에 아이템 정보 전달
        BossRewardInteractable rewardScript = rewardObj.GetComponent<BossRewardInteractable>();
        if (rewardScript != null)
        {
            rewardScript.SetupReward(lootList);
        }
    }
}