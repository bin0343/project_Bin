using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Loot : MonoBehaviour
{
    public static UI_Loot Instance;
    public GameObject lootPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void Open(List<ItemData> drops)
    {
        lootPanel.SetActive(true);
        // drops 리스트를 UI에 표시하는 로직 작성
    }
}
