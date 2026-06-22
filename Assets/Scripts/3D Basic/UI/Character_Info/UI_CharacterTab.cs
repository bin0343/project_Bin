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

    private void OnDisable()
    {
        if (currentSpawnedModel != null) currentSpawnedModel.SetActive(false);
        if (characterStageRoot != null) characterStageRoot.SetActive(false);
    }

    public void RefreshTab()
    {
        if (characterStageRoot != null) characterStageRoot.SetActive(true);

        mainInfoSubPanel.SetActive(true);
        if (enhancementSubPanel != null) enhancementSubPanel.SetActive(false);

        UpdateMainInfo();

        Character_Stat activeStat = UI_Manager.instance.activeCharacterStat;
        if (activeStat != null && activeStat.characterData != null)
        {
            SpawnAndSetupCharacter(activeStat.characterData);
        }
    }

    public void UpdateMainInfo()
    {
        Character_Stat activeStat = UI_Manager.instance.activeCharacterStat;

        if (activeStat != null)
        {
            CharacterStatus cStatus = Character_Manager.instance.GetCharacterStatus(activeStat.characterData.characterID);

            levelText.text = $"LV. {cStatus.level}";
            hpText.text = $"HP: {activeStat.currentHP} / {activeStat.maxHP}";
            attackText.text = $"공격력: {activeStat.attackPower}";
            defenseText.text = $"방어력: {activeStat.defensePower}";
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

        if (UI_CharacterEnhancement.instance != null)
        {
            UI_CharacterEnhancement.instance.OpenEnhancement();
        }
    }

    public void OnClickBackToInfo()
    {
        enhancementSubPanel.SetActive(false);
        mainInfoSubPanel.SetActive(true);
        UpdateMainInfo();
    }

    private IEnumerator PlayAppearAnimationDelyaed()
    {
        yield return null;

        uiCharacterAnimator.Play("First Idle", -1, 0f);
    }
}
