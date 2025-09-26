using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MonsterUIManager : MonoBehaviour
{
    private Transform player;
    public Transform monsterHPPanel;
    public float displayDistance = 10f;

    private GameObject currentTarget;
    private Slider hpSlider;
    private Text nameText;
    private Enemy_Stat currentStat;

    private Coroutine hpChangeRoutine; // 코루틴 핸들

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        monsterHPPanel.gameObject.SetActive(false);
        hpSlider = monsterHPPanel.GetComponentInChildren<Slider>();
        nameText = monsterHPPanel.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            float distance = Vector3.Distance(player.position, currentTarget.transform.position);

            if (distance <= displayDistance && currentStat.currentHP > 0)
            {
                if (!monsterHPPanel.gameObject.activeSelf)
                    monsterHPPanel.gameObject.SetActive(true);

                nameText.text = currentStat.EnemyName;
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
        }
    }

    public void SetTarget(GameObject monster)
    {
        currentTarget = monster;
        currentStat = monster.GetComponent<Enemy_Stat>();

        hpSlider.value = (float)currentStat.currentHP / currentStat.maxHP;
    }

    public void UpdateHPBar(int prevHP, int currentHP, int maxHP)
    {
        float startValue = (float)prevHP / maxHP;
        float targetValue = (float)currentHP / maxHP;

        if (hpChangeRoutine != null)
            StopCoroutine(hpChangeRoutine);

        hpChangeRoutine = StartCoroutine(AnimateHPBar(startValue, targetValue));
    }

    private IEnumerator AnimateHPBar(float startValue, float targetValue)
    {
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hpSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        hpSlider.value = targetValue;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        currentStat = null;
        monsterHPPanel.gameObject.SetActive(false);

        if (hpChangeRoutine != null)
            StopCoroutine(hpChangeRoutine);
    }
}
