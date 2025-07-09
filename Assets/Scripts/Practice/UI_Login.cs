using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
{
    public Text TEXTINPUT;
    public Text SaveText;

    //public string UserName;

    void Start()
    {
        /*string username = Shared.SceneMANAGER.GetPlayerPrefsStringKey("PlayerName");
        if (username.Length > 2)
        {
            Shared.SceneMANAGER.ChangeScene(SCENE.LOADING);
        }*/
    }

    public void OnBtnLogin()
    {
        if (TEXTINPUT.text.Length < 2)
            return;
        
        Shared.SceneMANAGER.UserName = TEXTINPUT.text;
        Shared.SceneMANAGER.SetPlayerPrefsStringKey("PlayerName", Shared.SceneMANAGER.UserName);

        Shared.SceneMANAGER.ChangeScene(SCENE.LOADING);
    }
}
