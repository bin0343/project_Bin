using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_Lobby : MonoBehaviour
{
    public Text TEXTUSERNAME;

    // Start is called before the first frame update
    void Start()
    {
        //TEXTUSERNAME.text = Shared.SceneMANAGER.UserName;
        SetVideo();
    }

    // Update is called once per frame

    public void Skip()
    {
        SkipVideo();
    }

    public void OnBtnLobby()
    {
        //SceneManager.LoadScene("Login");
        Shared.SceneMANAGER.ChangeScene(SCENE.TITLE);
    }
}
