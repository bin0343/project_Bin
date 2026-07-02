using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AttackVFX : MonoBehaviour
{
    [System.Serializable]
    public class SlashVFXData
    {
        [Header("Slash Prefab")]
        public GameObject slashPrefab;

        [Header("Spawn Point")]
        public Transform spawnPoint;

        [Header("Offset 보정")]
        public Vector3 positionOffset;
        public Vector3 rotationOffsetEuler;
        public Vector3 scale = Vector3.one;

        [Header("생성 후 제거 시간")]
        public float destroyTime = 1.0f;

        [Header("생성 후 SpawnPoint에 붙일지")]
        public bool parentToSpawnPoint = false;
    }

    [Header("콤보별 Slash VFX")]
    [SerializeField] private SlashVFXData[] slashVFXDatas = new SlashVFXData[3];

    private Player_Action playerAction;

    private void Awake()
    {
        playerAction = GetComponentInParent<Player_Action>();
    }

    public void PlaySlashVFX()
    {
        int comboIndex = GetComboIndex();

        if (comboIndex < 0 || comboIndex >= slashVFXDatas.Length) return;

        SlashVFXData data = slashVFXDatas[comboIndex];
        
        if (data == null) return;

        if (data.slashPrefab == null) return;

        Transform spawnPoint = data.spawnPoint != null ? data.spawnPoint : transform;

        Vector3 spawnPosition = spawnPoint.position + spawnPoint.TransformDirection(data.positionOffset);
        Quaternion spawnRotation = spawnPoint.rotation * Quaternion.Euler(data.rotationOffsetEuler);

        GameObject vfx = Instantiate(data.slashPrefab, spawnPosition, spawnRotation);

        vfx.transform.localScale = data.scale;

        if (data.parentToSpawnPoint)
        {
            vfx.transform.SetParent(spawnPoint);
        }

        if (data.destroyTime > 0)
        {
            Destroy(vfx, data.destroyTime);
        }
    }

    private int GetComboIndex()
    {
        if (playerAction == null) return 0;

        return Mathf.Clamp(playerAction.currentComboStep - 1, 0, 2);
    }
}
