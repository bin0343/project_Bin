using UnityEngine;

public static class Shared
{
    //static 정적변수, 메모리가 정해져있음. 프로그램이 종료될때 같이 종료, static안에선 static 변수만 사용가능
    public static SceneMANAGER SceneMANAGER;            //매니저들만 싱글톤으로 만들어서 싱글톤을 기록하는 걸 복잡하지 않게 가능
    public static PortalManager PotalManager;
    public static BattleManager BattleManager;
    public static BulletManager BulletManager;
    public static UI_Battle UIBattle;
}
