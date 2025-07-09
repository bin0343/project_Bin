using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUImanager : MonoBehaviour
{
    public GameObject inventoryPanel;
    private bool isOpen = false;

    private InventoryUI inventoryUI;

    private void Start()
    {
        inventoryUI = GetComponentInChildren<InventoryUI>();
    }

    void Awake()
    {
        if (FindObjectsOfType<InventoryUImanager>().Length > 1)
        {
            Destroy(gameObject); // 중복 제거
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
        }
    }
}
