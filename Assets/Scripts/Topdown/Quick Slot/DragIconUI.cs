using UnityEngine;
using UnityEngine.UI;

public class DragIconUI : MonoBehaviour
{
    public static DragIconUI Instance;

    public Image iconImage;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        iconImage.sprite = null;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }
}