using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Title : MonoBehaviour
{
    private Stat stat;
    // Start is called before the first frame update
    void Start()
    {
        stat = FindObjectOfType<Stat>();
        Shared.SceneMANAGER.SaveFile();
        Shared.SceneMANAGER.Init(stat);
    }

    public void OnBtnTitle ()
    {
        //SceneManager.LoadScene("Login");
        Shared.SceneMANAGER.ChangeScene(SCENE.LOGIN);
    }
}
