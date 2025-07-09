using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoUIManager : MonoBehaviour
{
    public GameObject CharacterInformation;
    private bool isOpen = false;
    public Button ExitButton;

    void Start()
    {
        ExitButton.onClick.AddListener(() =>
        {
            isOpen = !isOpen;
            CharacterInformation.SetActive(isOpen);
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            isOpen = !isOpen;
            CharacterInformation.SetActive(isOpen);
        }
    }
}
