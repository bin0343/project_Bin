using System.Collections.Generic;
using UnityEngine;

public class UI_PartyManager : MonoBehaviour
{
    public UI_MiniPortrait[] partySlots = new UI_MiniPortrait[3];

    void Update()
    {
        UpdatePartyUI();
    }

    public void UpdatePartyUI()
    {
        if (partySlots == null || partySlots.Length == 0) return;

        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] != null)
            {
                partySlots[i].gameObject.SetActive(false);
            }
        }

        if (Character_Manager.instance == null || Character_Manager.instance.currentPartyData == null) return;
        
        List<Character_Data> partyData = Character_Manager.instance.currentPartyData;

        for (int i = 0; i < partyData.Count; i++)
        {
            if (i >= partySlots.Length) break;

            Character_Data charData = partyData[i];

            if (charData != null && partySlots[i] != null)
            {
                partySlots[i].gameObject.SetActive(true); // 활성화

                float currentHp = GetCharacterCurrentHp(charData.characterID);
                float maxHp = GetCharacterMaxHp(charData.characterID);

                partySlots[i].UpdatePortrait(charData.characterPortrait, currentHp, maxHp);
            }
        }
    }

    private float GetCharacterCurrentHp(string charID)
    {
        if (UI_Manager.instance != null && UI_Manager.instance.activeCharacterStat != null && UI_Manager.instance.activeCharacterStat.characterData != null)
        {
            if (UI_Manager.instance.activeCharacterStat.characterData.characterID == charID)
            {
                return UI_Manager.instance.activeCharacterStat.currentHP;
            }
        }

        if (BattleManager.instance != null && BattleManager.instance.SpawnedCharacters != null)
        {
            foreach (GameObject charObj in BattleManager.instance.SpawnedCharacters)
            {
                if (charObj != null)
                {
                    Character_Stat stat = charObj.GetComponent<Character_Stat>();
                    if (stat != null && stat.characterData != null && stat.characterData.characterID == charID)
                    {
                        return stat.currentHP;
                    }
                }
            }
        }

        return GetCharacterMaxHp(charID);
    }

    private float GetCharacterMaxHp(string charID)
    {
        CharacterStatus status = Character_Manager.instance.GetCharacterStatus(charID);
        if (status != null && status.currentStats != null && status.currentStats.Length > (int)STAT.HP)
        {
            return status.currentStats[(int)STAT.HP];
        }
        return 100f; // 기본값
    }
}