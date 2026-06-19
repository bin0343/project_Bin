using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Player Data")]
public class Player_Data : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon;
    public GameObject uiPrefab;
}
