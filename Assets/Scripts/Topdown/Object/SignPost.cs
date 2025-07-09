using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignPost : MonoBehaviour
{
    public GameObject interactionUI; // "상호작용 : F"
    public GameObject descriptionPanel; // 설명창 전체 패널
    public Text descriptionText; // 텍스트 영역
    [TextArea]
    public string message; // 에디터에서 입력할 설명 텍스트
    public RectTransform interactionUIRect; // interactionUI의 RectTransform
    public Transform signTop; // 간판 위쪽 기준 위치 (사인포스트의 Transform 등)


    private bool isPlayerInRange = false;
    private bool isPanelOpen = false;

    private void Start()
    {
        interactionUI.SetActive(false);
        descriptionPanel.SetActive(false);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.G))
        {
            isPanelOpen = !isPanelOpen;
            descriptionPanel.SetActive(isPanelOpen);

            if (isPanelOpen)
            {
                descriptionText.text = message;
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
            Vector3 worldPos = signTop.position + new Vector3(0, -0.5f, 0); // 간판 기준 아래쪽
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            interactionUIRect.position = screenPos;
        }
    }
}
