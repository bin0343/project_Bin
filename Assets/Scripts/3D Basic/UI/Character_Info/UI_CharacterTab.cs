using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_CharacterTab : MonoBehaviour
{
    [Header("서브 패널")]
    public GameObject mainInfoSubPanel;
    public GameObject enhancementSubPanel;

    [Header("캐릭터 정보")]
    public Text levelText;
    public Text hpText;
    public Text attackText;
    public Text defenseText;

    [Header("3D 캐릭터 애니메이션")]
    [Tooltip("Render Texture 화면에 배치되어 있는 UI 전용 3D 캐릭터의 Animator를 연결")]
    public Animator uiCharacterAnimator;

    public GameObject characterStageRoot;

    private Dictionary<string, GameObject> cachedCharacterModels = new Dictionary<string, GameObject>();

    [Header("동적 모델 생성")]
    public Transform characterSpawnPoint;
    private GameObject currentSpawnedModel;
    public UI_CharacterRotation rotationScript;

    public Button characterEnhanceButton;
    private Character_Data currentlySelectedCharacter;

    private void OnDisable()
    {
        if (currentSpawnedModel != null) currentSpawnedModel.SetActive(false);
        if (characterStageRoot != null) characterStageRoot.SetActive(false);
    }

    public void RefreshTab(Character_Data selectedChar)
    {
        if (characterStageRoot != null) characterStageRoot.SetActive(true);

        mainInfoSubPanel.SetActive(true);
        if (enhancementSubPanel != null) enhancementSubPanel.SetActive(false);

        currentlySelectedCharacter = selectedChar;

        if (currentlySelectedCharacter != null)
        {
            DisplayCharacterInfo(currentlySelectedCharacter);
        }
    }

    public void DisplayCharacterInfo(Character_Data characterData)
    {
        if (characterData == null) return;

        currentlySelectedCharacter = characterData;

        CharacterStatus cStatus = Character_Manager.Instance.GetCharacterStatus(characterData.characterID);
        levelText.text = $"LV. {cStatus.level}";

        Character_Stat spawnedStat = null;
        if (BattleManager.Instance != null && BattleManager.Instance.SpawnedCharacters != null)
        {
            foreach (GameObject charObj in BattleManager.Instance.SpawnedCharacters)
            {
                if (charObj != null)
                {
                    Character_Stat stat = charObj.GetComponent<Character_Stat>();
                    if (stat != null && stat.characterData != null && stat.characterData.characterID == characterData.characterID)
                    {
                        spawnedStat = stat;
                        break;
                    }
                }
            }
        }

        // 실시간 월드 데이터 가공 표기 분기 처리
        if (spawnedStat != null)
        {
            // 출전 중인 파티원인 경우: 월드 오브젝트의 실시간 장비 연산 및 깎여 있는 현재 체력 데이터 정확히 로드
            hpText.text = $"HP: {spawnedStat.currentHP} / {spawnedStat.maxHP}";
            attackText.text = $"공격력: {spawnedStat.attackPower}";
            defenseText.text = $"방어력: {spawnedStat.defensePower}";
        }
        else
        {
            // 순수 대기실 소속 비파티원 멤버인 경우: 매니저 스탯을 기준으로 온전한 상태 연산 후 출력 (체력은 풀피)
            float maxHp = (cStatus.currentStats != null && cStatus.currentStats.Length > (int)STAT.HP) ? cStatus.currentStats[(int)STAT.HP] : 100f;
            float atk = (cStatus.currentStats != null && cStatus.currentStats.Length > (int)STAT.Attack) ? cStatus.currentStats[(int)STAT.Attack] : 10f;
            float def = (cStatus.currentStats != null && cStatus.currentStats.Length > (int)STAT.Defense) ? cStatus.currentStats[(int)STAT.Defense] : 5f;

            hpText.text = $"HP: {(int)maxHp} / {(int)maxHp}";
            attackText.text = $"공격력: {(int)atk}";
            defenseText.text = $"방어력: {(int)def}";
        }

        // 4. 하단 무대 영역의 3D 프리뷰 모델 동적 스위칭
        SpawnAndSetupCharacter(characterData);
    }

    public void UpdateMainInfo()
    {
        if (currentlySelectedCharacter != null)
        {
            DisplayCharacterInfo(currentlySelectedCharacter);
        }
    }

    public void SpawnAndSetupCharacter(Character_Data characterData)
    {
        if (characterData == null || characterData.uiPrefab == null || characterSpawnPoint == null) return;

        if (currentSpawnedModel != null)
        {
            currentSpawnedModel.SetActive(false);
        }

        if (cachedCharacterModels.ContainsKey(characterData.characterID))
        {
            currentSpawnedModel = cachedCharacterModels[characterData.characterID];
            currentSpawnedModel.SetActive(true);
        }
        else
        {
            currentSpawnedModel = Instantiate(characterData.uiPrefab, characterSpawnPoint.position, characterSpawnPoint.rotation);
            currentSpawnedModel.transform.SetParent(characterSpawnPoint);

            currentSpawnedModel.transform.localPosition = Vector3.zero;
            currentSpawnedModel.transform.localScale = Vector3.one;

            cachedCharacterModels.Add(characterData.characterID, currentSpawnedModel);
        }

        uiCharacterAnimator = currentSpawnedModel.GetComponent<Animator>();
        if (uiCharacterAnimator == null) uiCharacterAnimator = currentSpawnedModel.GetComponentInChildren<Animator>();

        if (uiCharacterAnimator != null)
        {
            uiCharacterAnimator.Rebind();
            uiCharacterAnimator.Play("First Idle", 0, 0f);
            uiCharacterAnimator.Update(0f);
        }

        UpdateRotationTarget();
    }

    private void UpdateRotationTarget()
    {
        if (rotationScript != null && currentSpawnedModel != null)
        {
            rotationScript.SetTarget(currentSpawnedModel.transform, uiCharacterAnimator);
        }
    }

    public void OnClickEnhanceCharacter()
    {
        mainInfoSubPanel.SetActive(false);
        enhancementSubPanel.SetActive(true);

        if (UI_CharacterEnhancement.Instance != null)
        {
            UI_CharacterEnhancement.Instance.OpenEnhancement(currentlySelectedCharacter);
        }
    }

    public void OnClickBackToInfo()
    {
        enhancementSubPanel.SetActive(false);
        mainInfoSubPanel.SetActive(true);
        UpdateMainInfo();
    }
}
