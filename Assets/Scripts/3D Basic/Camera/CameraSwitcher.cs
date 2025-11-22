using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Player_Move playerMove;
    [SerializeField] private GameObject dailyCameraSystem;
    [SerializeField] private GameObject combatCameraSystem;

    private bool isCombatMode = false;

    void Start()
    {
        if (playerMove == null)
        {
            playerMove = FindObjectOfType<Player_Move>();
        }
        SwitchToDailyMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (combatCameraSystem == null) return;

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
        if (dailyCameraSystem != null)
        {
            dailyCameraSystem.SetActive(true);
        }

        if (combatCameraSystem != null)
        {
            combatCameraSystem.SetActive(false);
        }
        isCombatMode = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMove != null)
        {
            playerMove.SetReferenceTransform(dailyCameraSystem.transform);
        }
    }
    
    public void SwitchToCombatMode()
    {
        if (dailyCameraSystem != null)
        {
            dailyCameraSystem.SetActive(false);
        }

        if (combatCameraSystem != null)
        {
            combatCameraSystem.SetActive(true);
        }
        isCombatMode = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMove != null)
        {
            playerMove.SetReferenceTransform(combatCameraSystem.transform);
        }
    }
}
