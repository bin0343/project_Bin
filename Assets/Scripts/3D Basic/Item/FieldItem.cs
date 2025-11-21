using System.Collections;
using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public Item_Base itemData;
    public SpriteRenderer spriteRendere;

    [Header("드랍 연출 설정")]
    public float dropForce = 5f;

    private bool isPickable = false;    //생성 직후 아이템 바로 먹기 방지

    public void Setup(Item_Base item)
    {
        itemData = item;
        if (spriteRendere != null && item.itemIcon != null)
        {
            spriteRendere.sprite = item.itemIcon;
        }
        transform.rotation = Quaternion.Euler(45f, 0, 0);

        StartCoroutine(DropAnimation());
    }

    //아이템 연출
    IEnumerator DropAnimation()
    {
        isPickable = false;

        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(randomDir * dropForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
        isPickable = true;

        //필요하면 물리 기능 끄기(최적화)
        if (rb != null) rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
    }

    public Item_Base GetItem()
    {
        if (!isPickable) return null;
        return itemData;
    }

    public void DestroyItem()
    {
        //획득 효과음
        Destroy(gameObject);
    }
}
