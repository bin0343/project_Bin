
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

public enum ENEMYSTATE
{
    IDLE,
    MOVE,
    SEARCH,
    ATTACK,
}

public enum BULLET
{
    NONE,
    LINE,   //직선
    TARGET  //유도
}