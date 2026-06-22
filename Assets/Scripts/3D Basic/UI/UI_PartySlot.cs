using UnityEngine;
using UnityEngine.UI;

public class UI_PartySlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image imgFace;
    public Text txtName; 
    public Text txtLevel;
    public GameObject selectedOverlay;

    [Header("편성 번호")]
    public GameObject numberBadge;
    public Image imgSlotNumber;
    public Sprite[] numberSprites;

    [HideInInspector] public Character_Data data;

    public void Setup(Character_Data characterData, int assignedSlotIndex = 0, bool isSelected = false)
    {
        data = characterData;

        if (selectedOverlay != null)
        {
            selectedOverlay.SetActive(isSelected);
        }

        if (numberBadge != null) numberBadge.SetActive(assignedSlotIndex > 0);

        if (imgSlotNumber != null)
        {
            imgSlotNumber.gameObject.SetActive(assignedSlotIndex > 0);

            if (assignedSlotIndex > 0 && numberSprites != null && assignedSlotIndex <= numberSprites.Length)
            {
                imgSlotNumber.sprite = numberSprites[assignedSlotIndex - 1];
            }
        }

        if (data != null)
        {
            imgFace.gameObject.SetActive(true);
            imgFace.sprite = data.characterPortrait;
            if (txtName != null) txtName.text = data.characterName;

            CharacterStatus status = Character_Manager.instance.GetCharacterStatus(data.characterID);
            if (txtLevel != null) txtLevel.text = $"Lv.{status.level}";
        }
        else
        {
            imgFace.gameObject.SetActive(false);
            if (txtName != null) txtName.text = "빈 자리";
            if (txtLevel != null) txtLevel.text = "";
        }
    }
}