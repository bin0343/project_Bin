using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && itemData != null)
        {
            Inventory inventory = other.GetComponent<Inventory>();

            if (inventory != null && itemData != null)
            {
                inventory.AddItem(itemData);
                Debug.Log($"'{itemData.itemName}'을(를) 인벤토리에 추가했습니다.");
                Destroy(gameObject);
            }
        }
    }
}