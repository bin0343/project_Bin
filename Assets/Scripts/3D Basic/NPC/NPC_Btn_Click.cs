using UnityEngine;
using UnityEngine.UI;

public class NPC_Btn_Click : MonoBehaviour
{
    [Header("NPC 설정")]
    public Character_Data npcData;  // 이 버튼이 어떤 NPC인지
    public int startDialogueID; // 시작할 대화 ID (CSV의 첫번째 ID)

    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickCharacter);

        // 버튼 이미지 투명 영역 클릭 방지
        Image img = GetComponent<Image>();
        if (img != null) img.alphaHitTestMinimumThreshold = 0.5f;
    }

    void OnClickCharacter()
    {
        if (DialogueManager.instance != null && npcData != null)
        {
            //DialogueManager.Instance.StartDialogue(startDialogueID, npcData);
        }
        else
        {
            Debug.LogError("DialogueManager가 없거나 NPC Data가 비어있습니다.");
        }
    }
}