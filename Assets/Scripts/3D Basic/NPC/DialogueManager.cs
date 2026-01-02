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
    public TextAsset csvFile;            // CSV 파일

    [Header("설정")]
    public float typingSpeed = 0.001f;    // 글자 나오는 속도

    // 내부 변수
    private Dictionary<int, DialogueEntry> dialogueDic = new Dictionary<int, DialogueEntry>();
    private NPC_Data currentTargetNPC;

    // 상태 관리 변수
    private bool isTyping = false;       // 현재 타이핑 중인가?
    private string fullText = "";        // 전체 텍스트 저장용
    private DialogueEntry currentEntry;  // 현재 대화 데이터
    private Coroutine typingCoroutine;   // 타이핑 코루틴 저장용

    private void Awake()
    {
        instance = this;
        dialogueRoot.SetActive(false);
        ParseCSV();
    }

    private void Update()
    {
        // 대화창이 꺼져있으면 클릭 감지 안 함
        if (!dialogueRoot.activeSelf) return;

        // 마우스 왼쪽 클릭 (모바일 터치 포함)
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 타이핑 중일 때 클릭 -> 즉시 전체 텍스트 출력
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                txtDialogue.text = fullText;
                isTyping = false;
            }
            // 2. 타이핑이 끝났는데 선택지가 안 뜬 상태에서 클릭 -> 선택지 보여주기
            else if (choiceGroup.gameObject.activeSelf == false)
            {
                ShowChoices();
            }
            // 3. 선택지가 이미 떠 있다면 -> 버튼을 눌러야 하므로 화면 클릭은 무시
        }
    }

    // --- CSV 파싱 (기존과 동일) ---
    void ParseCSV()
    {
        if (csvFile == null) return;
        List<Dictionary<string, object>> data = CSVReader.Read(csvFile);

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

                if (!dialogueDic.ContainsKey(id)) dialogueDic.Add(id, entry);
            }
            catch (System.Exception e) { Debug.LogError($"CSV 파싱 에러: {e.Message}"); }
        }
    }

    void AddChoice(DialogueEntry entry, object textObj, object nextIDObj, object effTypeObj, object effValObj)
    {
        string text = textObj.ToString();
        if (string.IsNullOrEmpty(text)) return;

        ChoiceData c = new ChoiceData();
        c.text = text;
        c.nextID = nextIDObj.ToString().Trim();
        c.effectType = effTypeObj.ToString().Trim();
        int.TryParse(effValObj.ToString(), out c.effectValue);
        entry.choices.Add(c);
    }

    // --- 대화 시작 ---
    public void StartDialogue(int startID, NPC_Data npcData)
    {
        currentTargetNPC = npcData;
        dialogueRoot.SetActive(true);

        if (npcData.standingIllust != null)
        {
            standingCG.sprite = npcData.standingIllust;
            standingCG.gameObject.SetActive(true);
        }
        else standingCG.gameObject.SetActive(false);

        ShowDialogueStep(startID);
    }

    // --- 단계별 대화 표시 (수정됨) ---
    void ShowDialogueStep(int id)
    {
        if (!dialogueDic.ContainsKey(id))
        {
            EndDialogue();
            return;
        }

        currentEntry = dialogueDic[id];
        txtName.text = currentEntry.speakerName;

        // [중요] 텍스트 타이핑 준비
        fullText = currentEntry.dialogueText.Replace("\\n", "\n");
        txtDialogue.text = ""; // 일단 비워둠

        // 선택지 그룹은 숨겨둠 (클릭해야 나옴)
        choiceGroup.gameObject.SetActive(false);

        // 이전 버튼들 청소
        foreach (Transform child in choiceGroup) Destroy(child.gameObject);

        // 타이핑 코루틴 시작
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    // 한글자씩 출력하는 코루틴
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

    // 선택지 버튼 생성 및 표시 (클릭 시 실행됨)
    void ShowChoices()
    {
        choiceGroup.gameObject.SetActive(true); // 이제 선택지 그룹 켜기

        if (currentEntry.choices.Count > 0)
        {
            foreach (ChoiceData choice in currentEntry.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceGroup);
                btnObj.GetComponentInChildren<Text>().text = choice.text;
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => OnChoiceClicked(choice));
            }
        }
        else
        {
            // 선택지가 없는 경우 (종료 버튼)
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceGroup);
            btnObj.GetComponentInChildren<Text>().text = "▼";
            btnObj.GetComponent<Button>().onClick.AddListener(() => EndDialogue());
        }
    }

    // --- 선택지 클릭 ---
    void OnChoiceClicked(ChoiceData choice)
    {
        ApplyEffect(choice.effectType, choice.effectValue);

        if (choice.nextID == "EXIT") EndDialogue();
        else ShowDialogueStep(int.Parse(choice.nextID));
    }

    void ApplyEffect(string type, int value)
    {
        if (value == 0) return;
        switch (type)
        {
            case "AFFINITY":
                NPC_Manager.instance.ChangeAffinity(currentTargetNPC.NPCID, value);
                break;
            case "NPC_STR":
                Debug.Log($"NPC 힘 {value} 증가");
                break;
        }
    }

    public void EndDialogue()
    {
        dialogueRoot.SetActive(false);
        currentTargetNPC = null;
    }
}