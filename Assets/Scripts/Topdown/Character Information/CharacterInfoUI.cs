using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoUI : MonoBehaviour
{
    private PlayerStat stat;

    public Slider HpSlider;
    public Slider MpSlider;
    public Slider ExpSlider;
    public Text HpText;
    public Text MpText;
    public Text AttackText;
    public Text DefenseText;
    public Text StatPointText;
    public Text ExpText;
    public Text PlayerLevel;

    public Button AttackPlusButton;
    public Button AttackMinusButton;
    public Button DefensePlusButton;
    public Button DefenseMinusButton;

    // Start is called before the first frame update
    void Start()
    {
        stat = FindObjectOfType<PlayerStat>();

        AttackPlusButton.onClick.AddListener(() => {
            stat.IncreaseAttack();
            UpdateStat();
        });

        AttackMinusButton.onClick.AddListener(() => {
            stat.DecreaseAttack();
            UpdateStat();
        });

        DefensePlusButton.onClick.AddListener(() => {
            stat.IncreaseDefense();
            UpdateStat();
        });

        DefenseMinusButton.onClick.AddListener(() => {
            stat.DecreaseDefense();
            UpdateStat();
        });
        
        UpdateStat();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStat();
    }

    public void UpdateStat()
    {
        HpSlider.maxValue = stat.MaxHp;
        HpSlider.value = stat.currentHp;
        MpSlider.maxValue = stat.MaxMp;
        MpSlider.value = stat.currentMp;
        ExpSlider.maxValue = stat.LevelUpExp;
        ExpSlider.value = stat.Exp;
        HpText.text = $"{stat.currentHp}/{stat.MaxHp}";
        MpText.text = $"{stat.currentMp}/{stat.MaxMp}";
        ExpText.text = $"{stat.Exp}/{stat.LevelUpExp}";
        PlayerLevel.text = "Level : " + stat.Level.ToString();
        if (stat != null)
        {
            AttackText.text = "공격력 : " + stat.Attack.ToString();
            DefenseText.text = "방어력 : " + stat.Defense.ToString();
            StatPointText.text = "스탯 포인트: " + stat.StatPoints.ToString();
        }
    }
}
