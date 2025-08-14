using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TableManager      //모노비헤이비어는 유니티에서 연결. 하지만 패치할때 쓰면 유니티 시작할때 문제가 생김. 패치를 해야하기 때문에 없어야됨. 유니티 에디터에 툴을 만들어서 사용.(데이터 만들때마다 데이터 뽑아야함, 메모리 직접 할당해줘야함.)
{
    public TableStage Stage = new TableStage();

    public void Init()
    {
#if UNITY_EDITOR
        Stage.Init_Csv("Book(Character)", 2, 0);
#else
        Stage.Init_Binary("Stage");
#endif
    }

    public void Save()
    {
        Stage.Save_Binary("Stage");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
