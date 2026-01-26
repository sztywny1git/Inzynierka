using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuHoverSwitcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image backgroundDisplay;
    public Sprite highlightedSprite;
    public Sprite defaultSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundDisplay.sprite = highlightedSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundDisplay.sprite = defaultSprite;
    }
}