using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterInfoManager : MonoBehaviour
{
    public static UI_CharacterInfoManager instance;

    [Header("탭")]
    public GameObject characterTabPanel;
    public GameObject weaponTabPanel;

    [Header("사이드 바 버튼")]
    public Button characterTabButton;
    public Button weaponTabButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
        SelectTab(0);
    }

    public void SelectTab(int tabIndex)
    {
        characterTabPanel.SetActive(false);
        weaponTabPanel.SetActive(false);

        if (tabIndex == 0)
        {
            characterTabPanel.SetActive(true);
            characterTabPanel.GetComponent<UI_CharacterTab>()?.RefreshTab();
        }
        else if (tabIndex == 1)
        {
            weaponTabPanel.SetActive(true);
            weaponTabPanel.GetComponent<UI_WeaponTab>()?.RefreshTab();
        }
    }
}
