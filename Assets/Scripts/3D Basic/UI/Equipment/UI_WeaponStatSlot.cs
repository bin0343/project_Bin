using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponStatSlot : MonoBehaviour
{
    public Text statNameText;   //스탯 이름 : 공격력, 방어력
    public Text currentValueText;   //현재수치
    public GameObject arrowObject;  //화살표 오브젝트(이미지)
    public Text previewValueText;   //레벨업 시 수치

    public void Setup(string statName, int currentValue, int previewValue, bool hasExpAdded)
    {
        statNameText.text = statName;
        currentValueText.text = currentValue.ToString();

        if (hasExpAdded && previewValue > currentValue)
        {
            if (arrowObject != null) arrowObject.SetActive(true);
            if (previewValueText != null)
            {
                previewValueText.gameObject.SetActive(true);
                previewValueText.text = previewValue.ToString();
                previewValueText.color = Color.green;
            }
        }
        else
        {
            if (arrowObject != null) arrowObject.SetActive(false);
            if (previewValueText != null) previewValueText.gameObject.SetActive(false);
        }
    }
}
