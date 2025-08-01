using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UI_Battle : MonoBehaviour
{
    private void Awake()
    {
        Shared.UIBattle = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnActionSkill(int _Index)
    {

    }

    public void OnBtnSKill(int _Index)
    {
        if (Shared.BattleManager == null)
            return;
    }
}
