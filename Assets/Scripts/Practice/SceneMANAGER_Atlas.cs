using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public partial class SceneMANAGER : MonoBehaviour
{
    [NonSerialized]
    Dictionary<string, SpriteAtlas> AtlasDic = new Dictionary<string, SpriteAtlas>();

    public Sprite GetSpriteAtlas (string _Prefab, string _Name)
    {
        if (AtlasDic.ContainsKey(_Prefab))
            return AtlasDic[_Prefab].GetSprite(_Name);

        UnityEngine.Object obj = null;

        obj = Resources.Load("Atlas/" + _Prefab);

        if (obj == null)
            return null;

        SpriteAtlas sa = obj as SpriteAtlas;

        if (sa != null)
        {
            AtlasDic.Add(_Prefab, sa);

            return sa.GetSprite(_Name);
        }

        return null;
    }
}
