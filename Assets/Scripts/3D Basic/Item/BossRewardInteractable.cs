using System.Collections.Generic;
using UnityEngine;

public class BossRewardInteractable : Interactable
{
    [Header("보상 설정")]
    public int requiredAP = 60; // 소모할 재화(레진/개척력 등)

    private List<Item_Base> rewardItems = new List<Item_Base>();
    private bool isLooted = false;

    // ItemDrop에서 생성될 때 아이템 리스트를 넘겨받는 함수
    public void SetupReward(List<Item_Base> items)
    {
        rewardItems = items;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isLooted) return;

        base.OnTriggerEnter(other);

        if (other.CompareTag("Player") && interactionText != null)
        {
            // 상호작용 텍스트 동적 변경
            interactionText.text = $"{interactionKey} : 보상 수령 (재화 {requiredAP} 소모)";
        }
    }

    // F키를 눌렀을 때 실행되는 함수
    protected override void OpenMenu()
    {
        if (isLooted) return;

        if (Account_Manager.Instance == null)
        {
            Debug.LogError("[BossReward] Account_Manager가 없습니다.");

            return;
        }

        if (Player_Inventory.Instance == null)
        {
            Debug.LogError("[BossReward] Player_Inventory가 없습니다.");

            return;
        }

        bool useSucceeded = Account_Manager.Instance.UseAP(requiredAP);

        if (!useSucceeded)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.ShowMessage("활동력이 부족합니다.");
            }

            Debug.Log($"[BossReward] 행동력 부족: " + $"{Account_Manager.Instance.currentAP}" + $"/{requiredAP}");

            return;
        }

        isLooted = true;

        foreach (Item_Base item in rewardItems)
        {
            if (item == null) continue;

            Player_Inventory.Instance.AddItem(item, 1);
        }

        if (BossArenaManager.Instance != null)
        {
            BossArenaManager.Instance.NotifyRewardClaimed();
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        isPlayerInRange = false;
        enabled = false;

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.ShowMessage($"행동력 {requiredAP}을 소모하여 " + "보상을 획득했습니다!");
        }

        Destroy(gameObject, 1.5f);
    }
}