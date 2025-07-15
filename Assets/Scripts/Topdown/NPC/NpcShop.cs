using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NpcShop : MonoBehaviour
{
    public GameObject interactionUI; 
    public GameObject descriptionPanel; 

    private bool isPlayerInRange = false;
    private bool isPanelOpen = false;

    public RectTransform interactionUIRect;
    public Transform ShopTop; 

    void Start()
    {
        interactionUI.SetActive(false);
        descriptionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.G))
        {
            isPanelOpen = !isPanelOpen;
            descriptionPanel.SetActive(isPanelOpen);

            if (isPanelOpen)
            {
                //descriptionText.text = message;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactionUI.SetActive(true);
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactionUI.SetActive(false);
            descriptionPanel.SetActive(false);
            isPlayerInRange = false;
            isPanelOpen = false;
        }
    }

    private void LateUpdate()
    {
        if (interactionUI.activeSelf)
        {
            Vector3 worldPos = ShopTop.position + new Vector3(0, -0.1f, 0); // 간판 기준 아래쪽
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            interactionUIRect.position = screenPos;
        }
    }
}
