using UnityEngine;
using UnityEngine.UI;

public class UI_AscensionMaterialSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;

    public void Setup(Item_Material material, int ownedCount, int requiredCount)
    {
        requiredCount = Mathf.Max(requiredCount, 1);
        ownedCount = Mathf.Max(ownedCount, 0);

        if (material == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null) iconImage.sprite = material.itemIcon;

        if (countText != null)
        {
            if (ownedCount >= requiredCount)
            {
                countText.text = $"<color=green>{ownedCount}</color> / " + $"{requiredCount}";
            }
            else
            {
                countText.text = $"<color=red>{ownedCount}</color> / " + $"{requiredCount}";
            }
        }
    }
}
