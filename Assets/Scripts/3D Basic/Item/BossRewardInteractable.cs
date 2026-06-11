using System.Collections.Generic;
using UnityEngine;

public class BossRewardInteractable : Interactable
{
    [Header("보상 설정")]
    public int requiredCurrency = 60; // 소모할 재화(레진/개척력 등)

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
            interactionText.text = $"{interactionKey} : 보상 수령 (재화 {requiredCurrency} 소모)";
        }
    }

    // F키를 눌렀을 때 실행되는 함수
    protected override void OpenMenu()
    {
        if (isLooted) return;

        bool hasEnoughCurrency = true; // 임시 테스트용 (나중에 실제 재화 로직으로 변경)

        if (hasEnoughCurrency)
        {
            isLooted = true;
            Debug.Log($"{requiredCurrency} 재화를 소모하여 보상을 획득했습니다!");

            foreach (var item in rewardItems)
            {
                if (Player_Inventory.instance != null)
                {
                    Player_Inventory.instance.AddItem(item, 1);
                }
            }

            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            isPlayerInRange = false;
            this.enabled = false;

            if (UI_Manager.instance != null)
            {
                UI_Manager.instance.ShowMessage("보상을 획득했습니다!");
            }

            Destroy(gameObject, 1.5f);
        }
        else
        {
            Debug.Log("재화가 부족하여 보상을 수령할 수 없습니다!");
            if (UI_Manager.instance != null)
            {
                UI_Manager.instance.ShowMessage("재화가 부족합니다!");
            }
        }
    }
}