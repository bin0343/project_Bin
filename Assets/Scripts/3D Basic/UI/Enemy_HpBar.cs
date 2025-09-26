using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_HpBar : MonoBehaviour
{
    [SerializeField] private Slider HpSlider;
    [SerializeField] private Text EnemyName;

    public void UpdateStatus(Enemy_Stat stat)
    {
        HpSlider.value = (float)stat.currentHP / stat.maxHP;
        EnemyName.text = stat.EnemyName;
    }
}
