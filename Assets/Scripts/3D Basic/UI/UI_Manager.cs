using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject StatusPanel;
    public UI_Status UI_Status;
    public UI_StatusBar UI_StatusBar;
    public Enemy_HpBar Enemy_HpBar;
    public Player_Stat PlayerStat;
    public Enemy_Stat EnemyStat;
    public GameObject InventoryPanel;
    public UI_Inventory UI_Inventory;

    private Stack<GameObject> UIStack = new Stack<GameObject>();

    private void Start()
    {
        if (PlayerStat == null)
        {
            PlayerStat = FindObjectOfType<Player_Stat>();
        }
        if (EnemyStat == null)
        {
            EnemyStat = FindObjectOfType<Enemy_Stat>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (StatusPanel.activeSelf)
            {
                CloseSpecificUI(StatusPanel);
            }
            else
            {
                UpdatePlayerStatus();
                OpenUI(StatusPanel);
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (InventoryPanel.activeSelf)
            {
                CloseSpecificUI(InventoryPanel);
            }
            else
            {
                OpenUI(InventoryPanel);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopUI();
        }
        UI_StatusBar.UpdateStatus(PlayerStat);
        Enemy_HpBar.UpdateStatus(EnemyStat);
    }

    public void UpdatePlayerStatus()
    {
        UI_Status.UpdateStatus(PlayerStat);
    }

    public void OpenUI(GameObject panel)
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
            UIStack.Push(panel);
        }
    }

    public void CloseTopUI()
    {
        if (UIStack.Count > 0)
        {
            GameObject topUI = UIStack.Pop();
            topUI.SetActive(false);
        }
    }

    public void CloseSpecificUI(GameObject panel)
    {
        if (panel.activeSelf)
        {
            panel.SetActive(false);
            Stack<GameObject> tempStack = new Stack<GameObject>();
            while (UIStack.Count > 0)
            {
                GameObject top = UIStack.Pop();
                if (top != panel)
                    tempStack.Push(top);
                else
                    break;
            }

            while (tempStack.Count > 0)
            {
                UIStack.Push(tempStack.Pop());
            }
        }
    }
}
