using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;

    public AccountSaveData account = new AccountSaveData();
    public List<CharacterSaveData> characters = new List<CharacterSaveData>();
    public PartySaveData party = new PartySaveData();
    public InventorySaveData inventory = new InventorySaveData();
    public WorldSaveData world = new WorldSaveData();
    public List<QuestSaveData> quests = new List<QuestSaveData>();
}

[Serializable]
public class AccountSaveData
{
    public string playerName;

    public int accountLevel = 1;
    public int accountExp;

    public int gold;

    public int currentAP = 240;
    public long lastAPUpdateUtcTicks;
}

[Serializable]
public class CharacterSaveData
{
    public string characterID;

    public bool isOwned;

    public int level = 1;
    public int currentExp;
    public int ascensionStage;

    public int currentHP;

    public ItemSaveData equippedWeapon;
}

[Serializable]
public class PartySaveData
{
    public List<string> currentPartyIDs = new List<string>();

    public int activeCharacterIndex;

    public List<PartyPresetSaveData> presets = new List<PartyPresetSaveData>();
}

[Serializable]
public class PartyPresetSaveData
{
    public string presetName;

    public List<string> characterIDs = new List<string>();
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items = new List<ItemSaveData>();

    public string quickSlotItemID;
}

[Serializable]
public class ItemSaveData
{
    public string itemID;
    public int quantity;
    public int slotIndex;

    // 무기일 경우 사용하는 개별 성장 데이터
    public int weaponLevel = 1;
    public int weaponExp = 0;
    public int breakthroughStage = 0;
    public int refinementStage = 1;
}

[Serializable]
public class WorldSaveData
{
    public string sceneName = "City";

    public bool hasSavedPosition = false;

    public Vector3 playerPosition;
    public float playerRotationY;

    public string lastTeleportPointID;

    public List<string> activatedTeleportIDs = new List<string>();
    public List<string> clearedBossIDs = new List<string>();
    public List<string> revealedLocationIDs = new List<string>();
    public List<CustomMapPinSaveData> customMapPins = new List<CustomMapPinSaveData>();
}

[Serializable]
public class CustomMapPinSaveData
{
    public Vector2 mapPos;
    public int iconIndex;
    public string description;
}