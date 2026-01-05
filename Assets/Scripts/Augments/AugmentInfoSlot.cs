using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AugmentInfoSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*public Image Icon;
    public TMP_Text Name;
    public TMP_Text Description; */
    
    public AugmentSO augmentSO;

    private AugmentInfo augmentInfo;


    void Start()
    {
        augmentInfo = FindObjectOfType<AugmentInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        if (augmentSO != null && augmentInfo != null && augmentInfo.infoAugmentPanel.alpha > 0)
        {
            augmentInfo.FollowMouse();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (augmentSO != null && augmentInfo != null)
        {
            augmentInfo.ShowAugmentInfo(augmentSO);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (augmentInfo != null)
        {
            augmentInfo.HideAugmentInfo();
        }
    }
}
