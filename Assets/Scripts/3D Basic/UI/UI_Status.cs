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

    public void UpdateStatus(Player_Stat stat)
    {
        hpSlider.value = (float)stat.CurrentHP / stat.MaxHP;
        mpSlider.value = (float)stat.CurrentMP / stat.MaxMP;

        hpText.text = $"{stat.CurrentHP} / {stat.MaxHP}";
        mpText.text = $"{stat.CurrentMP}  /  {stat.MaxMP}";
    }
}
