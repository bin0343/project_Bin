using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MonsterUIManager : MonoBehaviour
{
    //public GameObject monsterHPPrefab; // UI 프리팹
    private Transform player;
    public Transform monsterHPPanel;   // Canvas 내 패널 참조
    public float displayDistance = 10f;

    private GameObject currentTarget;
    private Slider hpSlider;
    private Text nameText;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        monsterHPPanel.gameObject.SetActive(false); 
        hpSlider = monsterHPPanel.GetComponentInChildren<Slider>();
        nameText = monsterHPPanel.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        GameObject nearest = FindNearestMonster();

        if (nearest != null)
        {
            float distance = Vector3.Distance(player.position, nearest.transform.position);
            if (distance <= displayDistance)
            {
                if (!monsterHPPanel.gameObject.activeSelf)
                    monsterHPPanel.gameObject.SetActive(true);

                Enemy_Stat stat = nearest.GetComponent<Enemy_Stat>();
                hpSlider.value = (float)stat.CurrentHP / stat.MaxHP;
                nameText.text = stat.EnemyName;

                currentTarget = nearest;
            }
            else
            {
                monsterHPPanel.gameObject.SetActive(false);
                currentTarget = null;
            }
        }
        else
        {
            monsterHPPanel.gameObject.SetActive(false);
            currentTarget = null;
        }
    }

    GameObject FindNearestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDistance)
            {
                nearest = monster;
                minDistance = dist;
            }
        }

        return nearest;
    }
}
