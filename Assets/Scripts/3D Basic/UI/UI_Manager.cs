using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject StatusPanel;
    public UI_Status UI_Status;
    public Player_Stat PlayerStat;

    private bool IsOpen = false;

    private void Start()
    {
        if (PlayerStat == null)
        {
            PlayerStat = FindObjectOfType<Player_Stat>();
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

    public void UpdatePlayerStatus()
    {
        UI_Status.UpdateStatus(PlayerStat);
    }

    private void Update()
    {
        ToggleStatusUI();
    }

    public void ToggleStatusUI()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            IsOpen = !IsOpen;
            StatusPanel.SetActive(IsOpen);
            UpdatePlayerStatus();
        }

    }
}
