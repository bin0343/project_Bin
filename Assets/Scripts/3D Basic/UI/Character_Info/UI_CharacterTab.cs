using UnityEngine;
using UnityEngine.UI;

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

    public Button characterEnhanceButton;

    public void RefreshTab()
    {
        mainInfoSubPanel.SetActive(true);
        enhancementSubPanel.SetActive(false);

        UpdateMainInfo();

        if (uiCharacterAnimator != null)
        {
            // Animator에 "ShowProfile" 이라는 Trigger가 설정되어 있어야 함
            // 없으면 무난하게 특정 상태를 직접 재생해도 됩니다: uiCharacterAnimator.Play("Pose_Idle");
            uiCharacterAnimator.SetTrigger("ShowProfile");
        }
    }

    public void UpdateMainInfo()
    {
        Player_Stat stat = Player_Stat.globalInstance;
        if (stat == null && UI_Manager.instance != null)
        {
            stat = UI_Manager.instance.playerStat;
        }

        if (stat != null)
        {
            levelText.text = $"LV. {stat.level}";
            hpText.text = $"HP: {stat.currentHP} / {stat.maxHP}";
            attackText.text = $"공격력: {stat.attackPower}";
            defenseText.text = $"방어력: {stat.defensePower}";
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
}
