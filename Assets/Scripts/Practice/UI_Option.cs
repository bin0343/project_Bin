using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Option : UI_Base
{
    private void Awake()
    {
    }

    private void OnEnable()
    {
        Shared.UIStack.Push(this);
    }
    private void ObDisable()
    {
        Shared.UIStack.Pop();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnBtnBgm(bool _Active)
    {
        if (_Active)    //사운드 키고
        {
            Debug.Log("OnBtnBgm : true");
        }
        else            //사운드 끄고
        {
            Debug.Log("OnBtnBgm : false");
        }
    }

    public void OnBtnExit()
    {
        gameObject.SetActive(false);
    }
}
