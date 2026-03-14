using System.Collections;
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
        Player_Stat localPlayer = null;
        if (UI_Manager.instance != null)
        {
            localPlayer = UI_Manager.instance.playerStat;
        }

        if (localPlayer != null && partySlots.Length > 0 && partySlots[0] != null)
        {
            Sprite pIcon = localPlayer.playerData != null ? localPlayer.playerData.characterIcon : null;

            partySlots[0].gameObject.SetActive(true);
            partySlots[0].UpdatePortrait(pIcon, localPlayer.currentHP, localPlayer.maxHP);
        }

        if (NPC_Manager.instance != null)
        {
            List<NPC_Data> partyData = NPC_Manager.instance.currentPartyData;

            for (int i = 0; i < 2; i++)
            {
                int slotIndex = i + 1;
                if (slotIndex >= partySlots.Length) break;

                if (i < partyData.Count && partyData[i] != null)
                {
                    NPC_Data npcData = partyData[i];
                    partySlots[slotIndex].gameObject.SetActive (true);

                    float currentHp = GetNpcCurrentHp(npcData.NPCID);
                    float maxHp = GetNpcMaxHp(npcData.NPCID);

                    partySlots[slotIndex].UpdatePortrait(npcData.NPCPortrait, currentHp, maxHp);
                }
                else
                {
                    // 해당 자리에 파티원이 없으면 UI를 숨김
                    if (partySlots[slotIndex] != null)
                        partySlots[slotIndex].gameObject.SetActive(false);
                }
            }
        }
    }

    private float GetNpcCurrentHp(string npcID)
    {
        NPC_Stat[] allNpcs = FindObjectsOfType<NPC_Stat>();
        foreach (var npc in allNpcs)
        {
            if (npc.npcData != null && npc.npcData.NPCID == npcID)
            {
                return npc.currentHP;
            }
        }

        return GetNpcMaxHp(npcID);
    }

    private float GetNpcMaxHp(string npcID)
    {
        NPCStatus status = NPC_Manager.instance.GetNPCStatus(npcID);
        if (status != null && status.currentStats != null)
        {
            return status.currentStats[(int)STAT.HP];
        }
        return 100f; // 기본값
    }
}
