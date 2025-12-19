using UnityEngine;
using UnityEngine.UI;

public class UI_PartySlot : MonoBehaviour
{
    [Header("UI ¿¬°á")]
    public Image imgFace;
    public Text txtName; 
    public Text txtLevel;
    public Text txtClass; 
    public GameObject selectedOverlay;

    [HideInInspector] public NPC_Data data;
    private bool isSelected = false;
    private ClubManager manager;

    public void Setup(NPC_Data npcData, bool selected, ClubManager mgr)
    {
        data = npcData;
        manager = mgr;
        isSelected = selected;

        if (data != null)
        {
            imgFace.sprite = data.NPCImage;
            txtName.text = data.NPCName;
            txtClass.text = data.classType.ToString();

            int lvl = NPC_Manager.instance.GetNPCStatus(data.NPCID, data).level;
            txtLevel.text = $"Lv.{lvl}";
        }

        UpdateSelectionUI();
    }

    public void OnClickSlot()
    {
        bool success = manager.OnSlotClicked(data.NPCID);

        if (success)
        {
            isSelected = !isSelected;
            UpdateSelectionUI();
        }
    }

    void UpdateSelectionUI()
    {
        if (selectedOverlay != null)
            selectedOverlay.SetActive(isSelected);
    }
}