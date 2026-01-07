using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI 연결")]
    public GameObject dialogueRoot;      // 대화창 전체 부모
    public Image standingCG;             // 캐릭터 이미지
    public Text txtName;                 // 이름
    public Text txtDialogue;             // 대화 텍스트
    public Transform choiceGroup;        // 선택지 버튼 그룹
    public GameObject choiceButtonPrefab;// 버튼 프리팹

    [Header("데이터")]
    public TextAsset[] csvFiles;            // CSV 파일

    [Header("설정")]
    public float typingSpeed = 0.05f;    // 글자 나오는 속도

    // 내부 변수
    private NPC_Data currentTargetNPC;

    // CSV 모드용 변수
    private Dictionary<int, DialogueEntry> dialogueDic = new Dictionary<int, DialogueEntry>();
    private DialogueEntry currentCSVEntry;

    // 퀘스트(Conversation) 모드용 변수
    private Conversation currentConversation;
    private bool isQuestMode = false; // 현재 모드 판별 (true: 퀘스트/SO, false: CSV)

    // 상태 관리 변수
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;

    private void Awake()
    {
        instance = this;
        dialogueRoot.SetActive(false);
        ParseCSV(); // CSV 데이터 미리 로드
    }

    private void Update()
    {
        if (!dialogueRoot.activeSelf) return;

        // 마우스 클릭 (타이핑 스킵 or 선택지 표시 or 대화 종료)
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타이핑 중 클릭 -> 즉시 전체 출력
                StopCoroutine(typingCoroutine);
                txtDialogue.text = fullText;
                isTyping = false;
            }
            else if (!choiceGroup.gameObject.activeSelf)
            {
                // 타이핑 끝났는데 선택지가 안 떴다면 -> 선택지 표시
                ShowChoices();
            }
            // 선택지가 떠있을 땐 버튼 클릭을 기다림 (아무것도 안 함)
            // 단, 결과(응답) 화면일 경우 클릭 시 종료 처리
            else if (choiceGroup.childCount == 0 && !isTyping)
            {
                EndDialogue();
            }
        }
    }

    // CSV 기반 대화 (일상 대화)
    public void StartDialogue(int startID, NPC_Data npcData)
    {
        isQuestMode = false; // CSV 모드
        currentConversation = null;
        currentTargetNPC = npcData;

        OpenDialogueUI();
        ShowCSVStep(startID);
    }

    void ShowCSVStep(int id)
    {
        if (!dialogueDic.ContainsKey(id))
        {
            EndDialogue();
            return;
        }

        foreach (Transform child in choiceGroup) Destroy(child.gameObject);
        choiceGroup.gameObject.SetActive(false);

        currentCSVEntry = dialogueDic[id];
        txtName.text = currentCSVEntry.speakerName;

        SetDialogueText(currentCSVEntry.dialogueText);
    }

    // ScriptableObject 기반 대화 (퀘스트)
    // NPC_Interaction에서 호출하는 함수
    public void StartConversation(Conversation convo, NPC_Data npcData, NPC_Interaction interactable = null)
    {
        if (convo == null) return;

        isQuestMode = true; // 퀘스트 모드
        currentCSVEntry = null;
        currentConversation = convo;
        currentTargetNPC = npcData;

        OpenDialogueUI();

        // 대화 내용 출력
        txtName.text = npcData.NPCName; // SO에는 화자 이름이 없으므로 NPC 데이터 사용
        SetDialogueText(convo.npcLine);
    }

    // UI 및 로직 처리

    void OpenDialogueUI()
    {
        dialogueRoot.SetActive(true);
        if (currentTargetNPC != null && currentTargetNPC.standingIllust != null)
        {
            standingCG.sprite = currentTargetNPC.standingIllust;
            standingCG.gameObject.SetActive(true);
        }
        else standingCG.gameObject.SetActive(false);

        // 이전 선택지 청소
        foreach (Transform child in choiceGroup) Destroy(child.gameObject);
        choiceGroup.gameObject.SetActive(false);
    }

    void SetDialogueText(string text)
    {
        fullText = text.Replace("\\n", "\n");
        txtDialogue.text = "";

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        foreach (char letter in fullText.ToCharArray())
        {
            txtDialogue.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // 선택지 버튼 생성 (모드에 따라 분기)
    void ShowChoices()
    {
        choiceGroup.gameObject.SetActive(true);

        // 1. 퀘스트 모드 (Conversation SO)
        if (isQuestMode && currentConversation != null)
        {
            if (currentConversation.playerChoice.Count > 0)
            {
                foreach (var choice in currentConversation.playerChoice)
                {
                    CreateChoiceButton(choice.choiceText, () => OnQuestChoiceClicked(choice));
                }
            }
            else
            {
                CreateChoiceButton("▼", () => EndDialogue());
            }
        }
        // 2. CSV 모드
        else if (!isQuestMode && currentCSVEntry != null)
        {
            if (currentCSVEntry.choices.Count > 0)
            {
                foreach (var choice in currentCSVEntry.choices)
                {
                    CreateChoiceButton(choice.text, () => OnChoiceClicked(choice));
                }
            }
            else
            {
                CreateChoiceButton("▼", () => EndDialogue());
            }
        }
    }

    void CreateChoiceButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choiceGroup);
        btnObj.GetComponentInChildren<Text>().text = text;
        btnObj.GetComponent<Button>().onClick.AddListener(action);
    }

    // 선택지 클릭 처리
    void OnChoiceClicked(ChoiceData choice)
    {
        ApplyEffect(choice.effectType, choice.effectValue);

        if (choice.nextID == "EXIT") EndDialogue();
        else ShowCSVStep(int.Parse(choice.nextID));
    }

    // [퀘스트] 선택지 클릭 처리
    void OnQuestChoiceClicked(DialogueChoice choice)
    {
        // 1. 친밀도 적용
        if (choice.affinityChange != 0)
            NPC_Manager.instance.ChangeAffinity(currentTargetNPC.NPCID, choice.affinityChange);

        // 2. 퀘스트 수락/완료 처리
        if (choice.questToStart != null)
            QuestManager.instance.AcceptQuest(choice.questToStart);

        if (choice.questToComplete != null)
            QuestManager.instance.ClaimReward(choice.questToComplete);

        // 3. NPC 응답 보여주기 (간단한 연출: 질문 텍스트를 응답으로 교체)
        // 선택지 버튼들은 숨김
        foreach (Transform child in choiceGroup) Destroy(child.gameObject);

        // 응답 텍스트 출력
        SetDialogueText(choice.npcResponse);

        // 응답 출력 후 클릭하면 종료되도록 설정 (Update문에서 처리됨)
    }

    // 효과 적용 (CSV용)
    void ApplyEffect(string type, string value)
    {
        if (string.IsNullOrEmpty(value) || value == "0") return;
        switch (type)
        {
            case "AFFINITY":
                if (int.TryParse(value, out int affinityVal))
                {
                    NPC_Manager.instance.ChangeAffinity(currentTargetNPC.NPCID, affinityVal);
                }
                break;
            case "NPC_STR":
                Debug.Log($"NPC 힘 {value} 증가");
                break;
            case "QUEST_START":
                // QuestManager에서 ID(문자열)로 퀘스트를 찾아 수락
                Quest qStart = QuestManager.instance.GetQuestByID(value);
                if (qStart != null) QuestManager.instance.AcceptQuest(qStart);
                break;

            // [추가] 퀘스트 완료(보상) 기능
            case "QUEST_COMPLETE":
                Quest qEnd = QuestManager.instance.GetQuestByID(value);
                if (qEnd != null) QuestManager.instance.ClaimReward(qEnd);
                break;
        }
    }

    public void EndDialogue()
    {
        dialogueRoot.SetActive(false);
        currentTargetNPC = null;
        currentConversation = null;
        currentCSVEntry = null;
    }

    // --- CSV 파싱 로직 (기존 유지) ---
    void ParseCSV()
    {
        if (csvFiles == null || csvFiles.Length == 0)
        {
            Debug.LogError("DialogueManager: 연결된 CSV 파일이 없습니다!");
            return;
        }
        foreach (TextAsset file in csvFiles)
        {
            if (file == null) continue;

            List<Dictionary<string, object>> data = CSVReader.Read(file);

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    int id = int.Parse(data[i]["ID"].ToString());
                    string speaker = data[i]["Speaker"].ToString();
                    string text = data[i]["Text"].ToString();

                    DialogueEntry entry = new DialogueEntry();
                    entry.id = id;
                    entry.speakerName = speaker;
                    entry.dialogueText = text;

                    if (data[i].ContainsKey("Choice1")) AddChoice(entry, data[i]["Choice1"], data[i]["NextID1"], data[i]["EffectType1"], data[i]["EffectValue1"]);
                    if (data[i].ContainsKey("Choice2")) AddChoice(entry, data[i]["Choice2"], data[i]["NextID2"], data[i]["EffectType2"], data[i]["EffectValue2"]);
                    if (data[i].ContainsKey("Choice3")) AddChoice(entry, data[i]["Choice3"], data[i]["NextID3"], data[i]["EffectType3"], data[i]["EffectValue3"]);

                    // 중복 ID 체크 (서로 다른 파일이라도 ID가 겹치면 안 됨)
                    if (!dialogueDic.ContainsKey(id))
                    {
                        dialogueDic.Add(id, entry);
                    }
                    else
                    {
                        Debug.LogWarning($"[주의] 중복된 대화 ID 발견: {id}. ({file.name})");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CSV 파싱 에러 ({file.name}): {e.Message}");
                }
            }
        }

        Debug.Log($"총 {dialogueDic.Count}개의 대화 데이터를 로드했습니다.");
    }

    void AddChoice(DialogueEntry entry, object textObj, object nextIDObj, object effTypeObj, object effValObj)
    {
        string text = textObj.ToString();
        if (string.IsNullOrEmpty(text)) return;

        ChoiceData c = new ChoiceData();
        c.text = text;
        c.nextID = nextIDObj.ToString().Trim();
        c.effectType = effTypeObj.ToString().Trim();
        c.effectValue = effValObj.ToString().Trim();
        entry.choices.Add(c);
    }
}