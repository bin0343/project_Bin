using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && itemData != null)
        {
            Debug.Log($"플레이어가 {itemData.itemName} 아이템({itemData.itemType})을 획득했습니다!");

            PlayerControl player = collision.GetComponent<PlayerControl>();

            if (player != null)
            {
                ApplyEffect(player);
            }

            Destroy(gameObject); // 사용형 아이템은 즉시 사라짐
        }
    }

    private void ApplyEffect(PlayerControl player)
    {
        switch (itemData.itemType)
        {
            case ItemType.Potion:
                if (itemData.itemName == "Healing Potion") 
                {
                    player.Heal(itemData.amount);
                }
                if (itemData.itemName == "Magic Potion")
                {
                    player.RecoverMp(itemData.amount);
                }
                break;

            case ItemType.Coin:
                // 예시: 플레이어의 코인 수 증가 (플레이어 스크립트에 메서드가 필요함)
                // player.AddCoins(itemData.amount);
                Debug.Log($"코인 {itemData.amount}개 획득!");
                break;

            case ItemType.Weapon:
                // 무기 장착 등 (별도 처리 필요)
                Debug.Log("무기를 즉시 장착합니다 (예시).");
                break;
        }
    }
}
