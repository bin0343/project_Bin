using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInteractionSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Sound")]
    [SerializeField] private bool useClickSound = true;
    [SerializeField] private bool useHoverSound = true;
    [SerializeField] private UISoundType clickSoundType = UISoundType.Click;

    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!useClickSound) return;

        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (selectable != null && !selectable.IsInteractable()) return;

        UIAudioManager.Instance?.Play(clickSoundType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!useHoverSound) return;

        if (selectable != null && !selectable.IsInteractable()) return;

        UIAudioManager.Instance?.Play(UISoundType.Hover);
    }
}