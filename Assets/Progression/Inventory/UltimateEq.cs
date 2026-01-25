using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UltimateEq : MonoBehaviour
{
    public CanvasGroup UltimateEqCanvas;

    //public GameObject UltimateShop;
    public GameObject EqAndAugmentsPanel;
    public GameObject StatsPanel;
    private bool UltimateEqPanel = false;

    public GameObject hotbar;
    public GameObject visibleStats;

    public AudioSource audioSource;
    public AudioClip inventoryOpenSound;


    void Start()
    {
        CloseEq();
    }

    
    private void Update()
    {
        if (OtherWindowIsOpen())
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UltimateEqPanel)
                CloseEq();
            else
                OpenEq();
        }
    }

    bool OtherWindowIsOpen()
    {
        return ShopKeeper.isShopOpen || AugmentManager.isAugmentSelectionOpen;
    }


    void OpenEq()
    {
        Time.timeScale = 0;

        audioSource.PlayOneShot(inventoryOpenSound);

        UltimateEqCanvas.alpha = 1;
        UltimateEqCanvas.blocksRaycasts = true;
        UltimateEqPanel = true;

        hotbar.SetActive(false);
        visibleStats.SetActive(false);
    }

    void CloseEq()
    {
        Time.timeScale = 1;

        audioSource.Stop();

        UltimateEqCanvas.alpha = 0;
        UltimateEqCanvas.blocksRaycasts = false;
        UltimateEqPanel = false;

        EqAndAugmentsPanel.SetActive(true);
        StatsPanel.SetActive(true);
        hotbar.SetActive(true);
        visibleStats.SetActive(true);
    }

}
