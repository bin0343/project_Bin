using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class FarmingRewardEntry
{
    public Item_Base item;

    [Min(1)]
    public int quantity = 1;
}

public class FarmingRewardInteractable : Interactable
{
    private UI_FarmingReward rewardUI;

    [Header("파밍 보상")]
    [SerializeField]
    private List<FarmingRewardEntry> rewards = new List<FarmingRewardEntry>();

    private bool isLooted;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (!other.CompareTag("Player")) return;

        if (interactionText != null)
        {
            interactionText.text = $"{interactionKey} : 보상 수령";
        }
    }

    protected override void OpenMenu()
    {
        if (rewardUI == null)
        {
            rewardUI = FindObjectOfType<UI_FarmingReward>(true);
        }

        if (rewardUI == null)
        {
            Debug.LogError("[FarmingReward] " + "UI_FarmingReward를 찾지 못했습니다.");

            return;
        }

        rewardUI.Prepare(this);

        OpenMenu(rewardUI.gameObject);
    }

    public void NotifyRewardPanelClosed()
    {
        isMenuOpen = false;
        currentOpenMenu = null;
    }

    public bool TryClaimReward(int apCost, int rewardMultiplier, out List<ItemHolder> grantedRewards)
    {
        grantedRewards = new List<ItemHolder>();

        if (isLooted) return false;

        if (Account_Manager.Instance == null)
        {
            Debug.LogError("[FarmingReward] Account_Manager가 없습니다.");

            return false;
        }

        if (Player_Inventory.instance == null)
        {
            Debug.LogError("[FarmingReward] Player_Inventory가 없습니다.");

            return false;
        }

        bool useSucceeded = Account_Manager.Instance.UseAP(apCost);

        if (!useSucceeded) return false;

        isLooted = true;

        foreach (FarmingRewardEntry reward in rewards)
        {
            if (reward == null || reward.item == null) continue;

            int finalQuantity = reward.quantity * rewardMultiplier;

            Player_Inventory.instance.AddItem(reward.item, finalQuantity, false);

            grantedRewards.Add(new ItemHolder(reward.item, finalQuantity));
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        isPlayerInRange = false;
        enabled = false;

        GameDataManager.Instance?.RequestAutoSave();

        return true;
    }
}