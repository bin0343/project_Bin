using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject dailyCameraSystem;
    [SerializeField] private GameObject combatCameraSystem;

    private bool isCombatMode = false;

    void Start()
    {
        SwitchToDailyMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isCombatMode = !isCombatMode;

            if (isCombatMode)
            {
                SwitchToCombatMode();
            }
            else
            {
                SwitchToDailyMode();
            }
        }
    }

    public void SwitchToDailyMode()
    {
        dailyCameraSystem.SetActive(true);
        combatCameraSystem.SetActive(false);
        isCombatMode = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void SwitchToCombatMode()
    {
        dailyCameraSystem.SetActive(false);
        combatCameraSystem.SetActive(true);
        isCombatMode = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
