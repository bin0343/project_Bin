using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGateInteraction : Interactable
{
    [Header("씬 이동")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnID = "BossEntrance";

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (!other.CompareTag("Player")) return;

        if (interactionText != null)
        {
            interactionText.text = $"{interactionKey} : 입장";
        }
    }

    protected override void OpenMenu()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[BossGate] SceneTransitionManager가 없습니다.");

            return;
        }

        isMenuOpen = true;

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        SceneTransitionManager.Instance.LoadScene(targetSceneName, targetSpawnID);
    }
}
