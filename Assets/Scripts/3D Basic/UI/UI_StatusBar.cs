using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatusBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;

    public void UpdateStatus(Player_Stat stat)
    {
        hpSlider.value = (float)stat.CurrentHP / stat.MaxHP;
        mpSlider.value = (float)stat.CurrentMP / stat.MaxMP;
    }
}
