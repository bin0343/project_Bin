using UnityEngine;

public class FarmingGateInteraction : Interactable
{
    [Header("파밍씬 이동")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnID = "FarmingEntry";

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
        if (SceneTransitionManager.instance == null)
        {
            Debug.LogError("[FarmingGate] SceneTransitionManager가 없습니다.");

            return;
        }

        isMenuOpen = true;

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        SceneTransitionManager.instance.LoadScene(targetSceneName, targetSpawnID);
    }
}