using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Status : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;
    [SerializeField] private Text hpText;
    [SerializeField] private Text mpText;

    public UI_EquipmentPanel uiEquipmentPanel;

    public void UpdateStatus(Player_Stat stat)
    {
        hpSlider.value = (float)stat.currentHP / stat.maxHP;
        mpSlider.value = (float)stat.currentMP / stat.maxMP;

        hpText.text = $"{stat.currentHP} / {stat.maxHP}";
        mpText.text = $"{stat.currentMP}  /  {stat.maxMP}";
    }
}
