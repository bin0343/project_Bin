using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stat : MonoBehaviour
{
    public string EnemyName;
    public int MaxHP = 80;
    public int CurrentHP = 80;

    public int AttackPower = 8;
    public int DefensePower = 3;

    public int ExpReward = 50;
    public MonsterHpBar hpBar;
    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);

    private EnemyBase enemyBase;

    void Start()
    {
        hpBar = GetComponentInChildren<MonsterHpBar>();
        if (hpBar != null)
            hpBar.Setup(this);

        myCanvas = GetComponentInChildren<Canvas>(true);
        enemyBase = GetComponent<EnemyBase>();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);

        if (CurrentHP > 0 && damage >= 1)
        {
            if (enemyBase != null)
            {
                enemyBase.Stun();
            }
        }

        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main.transform.rotation;
            Vector3 spawnPosition = transform.position + damageTextOffset;

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas);
        }

        if (hpBar != null)
            hpBar.UpdateHpBar();
    }
}
