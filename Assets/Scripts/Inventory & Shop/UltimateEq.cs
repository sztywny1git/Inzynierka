using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UltimateEq : MonoBehaviour
{
    public CanvasGroup UltimateEqCanvas;

    //public GameObject UltimateShop;
    public GameObject EqPanel;
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

    /*private void Update()
    {
        if (Input.GetButtonDown("ToggleUltimateEq"))
        {
            if (UltimateEqPanel)
            {
                Time.timeScale = 1; //pazuje gre

                //audioSource.PlayOneShot(inventoryOpenSound);

                UltimateEqCanvas.alpha = 0;
                UltimateEqCanvas.blocksRaycasts = false;
                UltimateEqPanel = false;

                //UltimateShop.SetActive(false);
                EqPanel.SetActive(true);
                StatsPanel.SetActive(true);

            }
            else
            {
                Time.timeScale = 1; //odpauzuje gre

                //audioSource.Stop();

                UltimateEqCanvas.alpha = 1;
                UltimateEqCanvas.blocksRaycasts = true;
                UltimateEqPanel = true;
            }
        }
    }*/

    private void Update()
    {
        if (ShopKeeperIsOpen())
            return;

        if (Input.GetButtonDown("ToggleUltimateEq"))
        {
            if (UltimateEqPanel)
                CloseEq();
            else
                OpenEq();
        }
    }

    bool ShopKeeperIsOpen()
    {
        return ShopKeeper.isShopOpen;
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

        EqPanel.SetActive(true);
        StatsPanel.SetActive(true);
        hotbar.SetActive(true);
        visibleStats.SetActive(true);
    }

}
