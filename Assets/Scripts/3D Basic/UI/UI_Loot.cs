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

    public void Toggle(List<ItemData> drops)
    {
        if (lootPanel.activeSelf)
        {
            UI_Manager.Instance.CloseSpecificUI(lootPanel);
        }
        else
        {
            UI_Manager.Instance.OpenUI(lootPanel);
        }
    }
}
