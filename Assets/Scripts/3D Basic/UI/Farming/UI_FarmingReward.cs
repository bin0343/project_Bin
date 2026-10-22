using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_FarmingReward : MonoBehaviour
{
    [Header("AP 비용")]
    [SerializeField] private int normalCost = 40;
    [SerializeField] private int doubleCost = 80;

    [Header("비용 텍스트")]
    [SerializeField] private Text normalCostText;
    [SerializeField] private Text doubleCostText;

    [Header("패널 연출")]
    [SerializeField] private RectTransform rewardWindow;
    [SerializeField, Min(0f)]
    private float slideDistance = 120f;
    [SerializeField, Min(0.01f)]
    private float openDuration = 0.18f;
    [SerializeField, Min(0.01f)]
    private float closeDuration = 0.14f;

    private Vector2 originalWindowPosition;
    private bool animationInitialized;

    [Header("보상 화면")]
    [SerializeField] private GameObject choiceContent;
    [SerializeField] private GameObject resultContent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform rewardSlotParent;

    [Header("결과 화면 AP")]
    [SerializeField] private Text resultAPText;

    private FarmingRewardInteractable currentInteractable;

    private void OnEnable()
    {
        RefreshCostState();
    }

    public void Prepare(FarmingRewardInteractable interactable)
    {
        currentInteractable = interactable;

        if (choiceContent != null)
        {
            choiceContent.SetActive(true);
        }

        if (resultContent != null)
        {
            resultContent.SetActive(false);
        }

        ClearRewardSlots();

        RefreshCostState();
    }

    private void RefreshCostState()
    {
        if (normalCostText != null)
        {
            normalCostText.text = normalCost.ToString();
        }

        if (doubleCostText != null)
        {
            doubleCostText.text = doubleCost.ToString();
        }

        if (Account_Manager.Instance == null) return;

        int currentAP = Account_Manager.Instance.currentAP;

        if (normalCostText != null)
        {
            normalCostText.color = currentAP >= normalCost ? Color.white : Color.red;
        }

        if (doubleCostText != null)
        {
            doubleCostText.color = currentAP >= doubleCost ? Color.white : Color.red;
        }
    }

    public void OnClickNormalReward()
    {
        TrySelectReward(normalCost, 1);
    }

    public void OnClickDoubleReward()
    {
        TrySelectReward(doubleCost, 2);
    }

    private void TrySelectReward(int apCost, int rewardMultiplier)
    {
        if (currentInteractable == null) return;

        List<ItemHolder> grantedRewards;

        bool success = currentInteractable.TryClaimReward(apCost, rewardMultiplier, out grantedRewards);

        if (!success)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.ShowMessage("AP가 부족합니다.");
            }

            RefreshCostState();

            return;
        }

        ShowRewardResult(grantedRewards);
    }

    private void ShowRewardResult(List<ItemHolder> grantedRewards)
    {
        if (choiceContent != null)
        {
            choiceContent.SetActive(false);
        }

        if (resultContent != null)
        {
            resultContent.SetActive(true);
        }

        RefreshResultAP();
        ClearRewardSlots();

        foreach (ItemHolder reward in grantedRewards)
        {
            if (reward == null || reward.ItemData == null) continue;

            GameObject slotObject = Instantiate(itemSlotPrefab, rewardSlotParent);

            UI_ItemSlot slot = slotObject.GetComponent<UI_ItemSlot>();

            if (slot == null)
            {
                Debug.LogError("[FarmingRewardUI] " + "ItemSlot Prefab에 UI_ItemSlot이 없습니다.");

                Destroy(slotObject);
                continue;
            }

            slot.SetDraggable(false);
            slot.Setup(reward);
        }
    }

    private void ClearRewardSlots()
    {
        if (rewardSlotParent == null) return;

        for (int i = rewardSlotParent.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardSlotParent.GetChild(i).gameObject);
        }
    }

    private void RefreshResultAP()
    {
        if (resultAPText == null) return;

        if (Account_Manager.Instance == null) return;

        int currentAP = Account_Manager.Instance.currentAP;

        int maxAP = Account_Manager.Instance.maxAP;

        if (currentAP <= 0)
        {
            resultAPText.text = $"<color=#FF4B4B>{currentAP}</color>/{maxAP}";
        }
        else
        {
            resultAPText.text = $"{currentAP}/{maxAP}";
        }
    }

    public void OnClickExit()
    {
        FarmingStageManager stageManager = FindFirstObjectByType<FarmingStageManager>();

        if (stageManager == null)
        {
            Debug.LogError("[FarmingRewardUI] FarmingStageManager를 찾지 못했습니다.");

            return;
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.CloseFarmingRewardAndThen(gameObject, stageManager.ExitStage);
        }
        else
        {
            stageManager.ExitStage();
        }
    }

    public void OnClickRetry()
    {
        FarmingStageManager stageManager = FindFirstObjectByType<FarmingStageManager>();

        if (stageManager == null)
        {
            Debug.LogError("[FarmingRewardUI] FarmingStageManager를 찾지 못했습니다.");

            return;
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.CloseFarmingRewardAndThen(gameObject, stageManager.RetryStage);
        }
        else
        {
            stageManager.RetryStage();
        }
    }

    #region UI 애니메이션

    private void InitializeAnimation()
    {
        if (animationInitialized) return;

        if (rewardWindow == null)
        {
            Debug.LogError("[FarmingRewardUI] Reward Window가 연결되지 않았습니다.");

            return;
        }

        originalWindowPosition = rewardWindow.anchoredPosition;

        animationInitialized = true;
    }

    public void PlayOpenAnimation()
    {
        InitializeAnimation();

        if (!animationInitialized) return;

        rewardWindow.DOKill();

        rewardWindow.anchoredPosition =
            originalWindowPosition +
            Vector2.down * slideDistance;

        rewardWindow.DOAnchorPos(originalWindowPosition, openDuration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public void PlayCloseAnimation(System.Action onComplete)
    {
        InitializeAnimation();

        if (!animationInitialized)
        {
            onComplete?.Invoke();
            return;
        }

        rewardWindow.DOKill();

        Vector2 hidePosition = originalWindowPosition + Vector2.down * slideDistance;

        rewardWindow.DOAnchorPos(hidePosition, closeDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                rewardWindow.anchoredPosition = originalWindowPosition;

                onComplete?.Invoke();
            });
    }

    #endregion

    private void OnDisable()
    {
        if (rewardWindow != null)
        {
            rewardWindow.DOKill();

            if (animationInitialized)
            {
                rewardWindow.anchoredPosition = originalWindowPosition;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.NotifyRewardPanelClosed();

            currentInteractable = null;
        }
    }
}