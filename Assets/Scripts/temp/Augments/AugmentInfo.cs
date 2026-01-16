using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class AugmentInfo : MonoBehaviour
{

    public CanvasGroup infoAugmentPanel;

    public Image augmentIcon;

    public TMP_Text augmentNameText;

    public TMP_Text augmentDescriptionText;

    private RectTransform infoAugmentRect;

    private void Awake()
    {
        infoAugmentRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        HideAugmentInfo();
    }

    public void ShowAugmentInfo(AugmentSO augmentSO)
    {
        augmentIcon.sprite = augmentSO.icon;
        augmentIcon.enabled = augmentSO.icon != null;

        augmentNameText.text = augmentSO.augmentName.ToString();

        augmentDescriptionText.text = augmentSO.description;

        infoAugmentPanel.alpha = 1;
    }

    public void HideAugmentInfo()
    {
        infoAugmentPanel.alpha = 0;
        augmentNameText.text = "";
        augmentDescriptionText.text = "";
    }

    public void FollowMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = new Vector3(0, 50, 0);
        infoAugmentRect.position = mousePosition + offset;
    }
}
