
public enum SCENE
{
    TITLE,
    LOGIN,
    LOADING,
    LOBBY,
    BATTLE,
    END
}

public enum LOADSTRING
{
    STRING1,
    STRING2,
    STRING3,
    STRING4,
    STRING5,
}

public enum AI
{
    AI_CREATE,
    AI_SEARCH,
    AI_MOVE,
    AI_RESET
}

/*public enum STAT
{
    STAT_HP,
    STAT_ATK,
    STAT_CRI,
    STAT_RATE,
    STAT_END
}*/

public enum BULLET
{
    NONE,
    LINE,   //직선
    TARGET  //유도
}

public enum SKILLTYPE
{
    Buff,
    Attack,
    Heal
}

public enum ITEMTYPE
{
    Equipment,
    Consumable,
    Material,
    Quest,
    ETC
}