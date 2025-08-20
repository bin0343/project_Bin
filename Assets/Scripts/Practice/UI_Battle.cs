using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public partial class UI_Battle : MonoBehaviour
{
    public JoyStick JOYSTICK;
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

    public void OnPointerDown(BaseEventData eventData)
    {
        JOYSTICK.gameObject.SetActive(true);

#if UNITY_ANDROID
#if UNITY_EDITOR
        JOYSTICK.transform.position = Input.mousePosition;

#else
        Touch touch = Input.GetTouch(0);
        JOYSTICK.transform.position = touch.position;
#endif
#endif
        JOYSTICK.OnDown((PointerEventData)eventData);
    }

    public void OnPointerUp(BaseEventData eventData)
    {
        JOYSTICK.gameObject.SetActive(false);
        JOYSTICK.OnUp((PointerEventData)eventData);
    }

    public void OnDrag(BaseEventData eventData)
    {
        JOYSTICK.OnDrag((PointerEventData)eventData);
    }
}
