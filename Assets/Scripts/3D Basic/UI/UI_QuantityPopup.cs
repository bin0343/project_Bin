using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UI_QuantityPopup : MonoBehaviour
{
    public Text quantityText;
    public Button confirmButton;
    public Button cancelButton;

    private int currentQuantity = 1;
    private int maxQuantity = 99;
    private ShopItem currentItem;

    private Action<ShopItem, int> onConfirm;
    private Action onCancelCallback;

    private enum PopupState
    {
        SelectingQuantity,      //수량 선택 중
        SelectingConfirmation   //구매/닫기 버튼 선택 중
    }
    private PopupState currentState;

    public void Initialize(ShopItem item, int maxBuyableAmount, Action<ShopItem, int> onComfirmCallback, Action onCancelCallback)
    {
        this.currentItem = item;
        this.onConfirm = onComfirmCallback;
        this.onCancelCallback = onCancelCallback;
        this.maxQuantity = maxBuyableAmount;
        this.currentQuantity = 1;

        UpdateQuantityText();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancel);

        currentState = PopupState.SelectingQuantity;

        EventSystem.current.SetSelectedGameObject(null);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (currentState == PopupState.SelectingQuantity)
        {
            HandleQuantityInput();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                currentState = PopupState.SelectingConfirmation;
                EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancel();
            }
        }
        else     //currentState == PopupState.SelectingConfirmation
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentState = PopupState.SelectingQuantity;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void HandleQuantityInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentQuantity++;
            if (currentQuantity > maxQuantity) currentQuantity = maxQuantity;
            UpdateQuantityText();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentQuantity--;
            if (currentQuantity < 1) currentQuantity = 1;
            UpdateQuantityText();
        }
        // 좌/우 방향키: +/- 10
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentQuantity -= 10;
            if (currentQuantity < 1) currentQuantity = 1;
            UpdateQuantityText();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentQuantity += 10;
            if (currentQuantity > maxQuantity) currentQuantity = maxQuantity;
            UpdateQuantityText();
        }
    }

    private void UpdateQuantityText()
    {
        quantityText.text = currentQuantity.ToString();
    }

    private void OnConfirm()
    {
        if (currentQuantity <= 0)
        {
            OnCancel();
            return;
        }
        onConfirm?.Invoke(currentItem, currentQuantity);
        gameObject.SetActive(false);
    }

    private void OnCancel()
    {
        onCancelCallback?.Invoke();
        gameObject.SetActive(false);
    }
}
