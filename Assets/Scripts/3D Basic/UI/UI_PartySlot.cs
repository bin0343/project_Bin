using UnityEngine;
using UnityEngine.UI;

public class UI_PartySlot : MonoBehaviour
{
    [Header("UI ¿¬°á")]
    public Image imgFace;
    public Text txtName; 
    public Text txtLevel;
    public GameObject selectedOverlay;

    [HideInInspector] public Character_Data data;

    public void Setup(Character_Data characterData, bool isSelected = false)
    {
        data = characterData;

        if (selectedOverlay != null)
        {
            selectedOverlay.SetActive(isSelected);
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
            if (txtName != null) txtName.text = "ºó ÀÚ¸®";
            if (txtLevel != null) txtLevel.text = "";
        }
    }
}