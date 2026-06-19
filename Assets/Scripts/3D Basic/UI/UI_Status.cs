using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Status : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;

    public UI_EquipmentPanel uiEquipmentPanel;

    public void UpdateStatus(Character_Stat stat)
    {
        hpSlider.value = (float)stat.currentHP / stat.maxHP;

        hpText.text = $"{stat.currentHP} / {stat.maxHP}";
    }
}
