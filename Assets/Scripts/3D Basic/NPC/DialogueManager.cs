using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI 요소")]
    public GameObject dialoguePanel;
    public Text npcNameText;
    public Image npcImage;
    public Text dialogueLineText;
    public Transform choiceButtonParent;    //선택지 버튼 생성될 부모
    public GameObject choiceButtonPrefab;
    public Text npcResponseText;    //선택 후 npc응답 텍스트

    private NPC_Data currentNpc;
    private List<GameObject> spawnedButton = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartConversation(Conversation convo, NPC_Data npc)     //대화 시작
    {
        currentNpc = npc;
        UI_Manager.Instance.OpenUI(dialoguePanel);

        npcNameText.text = npc.NPCName;
        npcImage.sprite = npc.NPCImage;
        dialogueLineText.text = convo.npcLine;
        npcResponseText.gameObject.SetActive(false);

        //기존 버튼 삭제
        foreach (GameObject button in spawnedButton)
        {
            Destroy(button);
        }
        spawnedButton.Clear();

        //새 버튼 생성
        foreach (DialogueChoice choice in convo.playerChoice)
        {
            GameObject buttonInstance = Instantiate(choiceButtonPrefab, choiceButtonParent);
            spawnedButton.Add(buttonInstance);

            //버튼 텍스트 설정
            var buttonText = buttonInstance.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            var button =buttonInstance.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnChoiceSelected(choice);
            });
        }

        if (spawnedButton.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedButton[0]);
        }
    }

    //선택지 버튼을 선택했을때
    private void OnChoiceSelected(DialogueChoice choice)
    {
        //1. 친밀도 변경
        NPC_Manager.instance.ChangeAffinity(currentNpc.NPCID, choice.affinityChange);

        // 2. 퀘스트 수락
        if (choice.questToStart != null)
        {
            QuestManager.instance.AcceptQuest(choice.questToStart);
            // (메시지는 퀘스트 매니저가 띄워주는 게 더 좋음)
        }

        // 3. 퀘스트 완료 (보상 받기)
        if (choice.questToComplete != null)
        {
            // (주의: 퀘스트가 COMPLETED 상태인지 확인하는 로직이 ClaimReward에 있음)
            QuestManager.instance.ClaimReward(choice.questToComplete);
        }

        //4. 선택지 버튼 숨기기
        foreach (GameObject button in spawnedButton)
        {
            button.SetActive(false);
        }

        //5. npc 응답 보여주기
        dialogueLineText.text = ""; //기존 질문 가리기
        npcResponseText.gameObject.SetActive(true);
        npcResponseText.text = choice.npcResponse;

        //6. 대화 창 닫기
        StartCoroutine(EndDialogueAfterDelay(2.0f));
    }

    private IEnumerator EndDialogueAfterDelay(float delay)
    {
        yield  return new WaitForSeconds(delay);
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        UI_Manager.Instance.CloseSpecificUI(dialoguePanel);
    }
}
