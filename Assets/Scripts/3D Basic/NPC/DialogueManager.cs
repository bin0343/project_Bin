using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI 요소")]
    public GameObject dialoguePanel;
    public Text npcNameText;
    public Image npcImage;
    public Text dialogueLineText;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;
    public Text npcResponseText;

    private NPC_Data currentNpcData;
    private NPC_Interaction currentInteractable; // 현재 대화 중인 상호작용 스크립트 기억

    private List<GameObject> spawnedButton = new List<GameObject>(); // 리스트 누락되어 추가함

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
        dialoguePanel.SetActive(false);
    }

    // StartConversation 오버로딩 (Interaction 스크립트를 받아옴)
    public void StartConversation(Conversation convo, NPC_Data npc, NPC_Interaction interactable = null)
    {
        currentNpcData = npc;
        currentInteractable = interactable; // 누가 대화를 걸었는지 저장

        UI_Manager.Instance.OpenUI(dialoguePanel);

        npcNameText.text = npc.NPCName;
        npcImage.sprite = npc.NPCImage;
        dialogueLineText.text = convo.npcLine;
        npcResponseText.gameObject.SetActive(false);

        // 버튼 초기화 및 생성 로직 (기존과 동일)
        foreach (Transform child in choiceButtonParent) Destroy(child.gameObject);
        spawnedButton.Clear();

        foreach (DialogueChoice choice in convo.playerChoice)
        {
            GameObject buttonInstance = Instantiate(choiceButtonPrefab, choiceButtonParent);
            spawnedButton.Add(buttonInstance);

            var buttonText = buttonInstance.GetComponentInChildren<Text>();
            if (buttonText != null) buttonText.text = choice.choiceText;

            var button = buttonInstance.GetComponent<Button>();
            button.onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        NPC_Manager.instance.ChangeAffinity(currentNpcData.NPCID, choice.affinityChange);

        if (choice.questToStart != null) QuestManager.instance.AcceptQuest(choice.questToStart);
        if (choice.questToComplete != null) QuestManager.instance.ClaimReward(choice.questToComplete);

        foreach (GameObject button in spawnedButton) button.SetActive(false);

        dialogueLineText.text = "";
        npcResponseText.gameObject.SetActive(true);
        npcResponseText.text = choice.npcResponse;

        // 2초 뒤 대화 종료 + 메뉴 열기 여부 판단
        StartCoroutine(EndDialogueAfterDelay(2.0f, choice.openInteractionMenu));
    }

    private IEnumerator EndDialogueAfterDelay(float delay, bool openMenu)
    {
        yield return new WaitForSeconds(delay);
        CloseDialogue();

        // 메뉴 열기가 체크된 선택지였다면 메뉴 오픈
        if (openMenu && currentInteractable != null)
        {
            currentInteractable.ShowInteractionMenu();
        }
    }

    public void CloseDialogue()
    {
        UI_Manager.Instance.CloseSpecificUI(dialoguePanel);
    }
}