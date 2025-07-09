using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    public GameObject GameMenu;
    public GameObject MenuPanel;
    public GameObject OptionPanel_Button;

    public GameObject OptionPanel_Control;
    public GameObject OptionPanel_Graphic;
    public GameObject OptionPanel_Sound;

    private GameObject[] optionPanels;

    public Button ResumeButton;
    public Button OptionButton;
    public Button MainMenuButton;
    public Button QuitButton;
    public Button BackButton;
    public Button CloseButton;
    public Button Control_Button;
    public Button Graphic_Button;
    public Button Sound_Button;

    private bool isMenuOpen = false;

    void Start()
    {
        GameMenu.SetActive(false);

        optionPanels = new GameObject[] 
        {
            OptionPanel_Control,
            OptionPanel_Graphic,
            OptionPanel_Sound
        };

        ResumeButton.onClick.AddListener(ResumeGame);
        OptionButton.onClick.AddListener(OpenOptions);
        MainMenuButton.onClick.AddListener(GoToMainMenu);
        QuitButton.onClick.AddListener(QuitGame);
        BackButton.onClick.AddListener(BackToMenu);
        CloseButton.onClick.AddListener(ResumeGame);

        Control_Button.onClick.AddListener(() => ShowOptionPanel(OptionPanel_Control));
        Graphic_Button.onClick.AddListener(() => ShowOptionPanel(OptionPanel_Graphic));
        Sound_Button.onClick.AddListener(() => ShowOptionPanel(OptionPanel_Sound));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        GameMenu.SetActive(isMenuOpen);
        MenuPanel.SetActive(isMenuOpen);
        Time.timeScale = isMenuOpen ? 0f : 1f;

        if (isMenuOpen)
        {
            Animator animator = MenuPanel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.Play("MenuOpen", 0, 0f);
            }
        }
    }

    void ShowOptionPanel(GameObject targetPanel)
    {
        foreach (var panel in optionPanels)
        {
            panel.SetActive(panel == targetPanel);
        }
    }

    void BackToMenu()
    {
        DeactivateAllOptionPanels();
        MenuPanel.SetActive(true);
        if (OptionPanel_Button.activeSelf == true)
            OptionPanel_Button.SetActive(false);

        Animator animator = MenuPanel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    void ResumeGame()
    {
        isMenuOpen = false;
        GameMenu.SetActive(false);
        Time.timeScale = 1f;

        DeactivateAllOptionPanels();
        OptionPanel_Button.SetActive(false);
    }

    void OpenOptions()
    {
        MenuPanel.SetActive(false);
        OptionPanel_Button.SetActive(true);
        ShowOptionPanel(OptionPanel_Control);
    }

    void DeactivateAllOptionPanels()
    {
        foreach (var panel in optionPanels)
        {
            panel.SetActive(false);
        }
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("MainMenu");
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}