using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    //플레이어 기본 정보
    public string playerName;
    public int playerLevel;
    public int playerGold;
    public int playerExp;
    public float[] baseStats;   //플레이어 기본 스탯

    //게임 진행 정보
    public int day;         // 날짜
    public string timeOfDay;// 시간대 (Morning, Afternoon 등)
    public bool isTutorialFinished; // 튜토리얼 끝났는지

    //인벤토리 (아이템)
    public List<ItemSaveData> inventoryList = new List<ItemSaveData>();
    public List<ItemSaveData> quickSlotList = new List<ItemSaveData>();

    //장착 장비
    public List<EquipSaveData> equipmentList = new List<EquipSaveData>();

    //NPC 상태
    public List<NPCSaveData> npcList = new List<NPCSaveData>();
}

//보조 데이터 구조체들

[System.Serializable]
public class ItemSaveData
{
    public string itemID;    // 아이템 식별자 (예: "POTION_RED")
    public int quantity;     // 개수
    public int slotIndex;    // 인벤토리 몇 번째 칸인가
}

[System.Serializable]
public class EquipSaveData
{
    public string equipSlot; // 장착 부위 (RightHand, LeftHand, Armor)
    public string itemID;    // 장착된 아이템 ID
}

[System.Serializable]
public class NPCSaveData
{
    public string npcID;     // NPC 식별자
    public int affinity;     // 호감도
    public int level;        // 레벨
    public int currentExp;   // 경험치
    public float[] npcStats;
}