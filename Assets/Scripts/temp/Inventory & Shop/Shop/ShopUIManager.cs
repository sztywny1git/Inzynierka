using UnityEngine;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    public CanvasGroup shopCanvasGroup;
    public ShopManager shopManager;

    public CanvasGroup UltimateEqCanvas;
    public GameObject EqPanel;
    public GameObject StatsPanel;
    public GameObject hotbar;
    public GameObject visibleStats;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleShop(List<ShopItems> items, AudioSource audioSource, AudioClip openSound)
    {
        if (!ShopKeeper.isShopOpen)
        {
            ShopKeeper.isShopOpen = true; 
            Time.timeScale = 0;

            if(audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            EqPanel.SetActive(false);
            StatsPanel.SetActive(false);
            hotbar.SetActive(false);
            visibleStats.SetActive(false);

            UltimateEqCanvas.alpha = 1;
            UltimateEqCanvas.blocksRaycasts = true;

            shopCanvasGroup.alpha = 1;
            shopCanvasGroup.blocksRaycasts = true;
            shopCanvasGroup.interactable = true;

            shopManager.PopulateShopItems(items);
            
            ShopKeeper.TriggerShopStateEvent(shopManager, true);
        }
        else
        {
            CloseShop();
        }
    }

    public void CloseShop()
    {
        ShopKeeper.isShopOpen = false;
        Time.timeScale = 1;

        shopCanvasGroup.alpha = 0;
        shopCanvasGroup.blocksRaycasts = false;
        shopCanvasGroup.interactable = false;

        UltimateEqCanvas.alpha = 0;
        UltimateEqCanvas.blocksRaycasts = false;

        EqPanel.SetActive(true);
        StatsPanel.SetActive(true);
        hotbar.SetActive(true);
        visibleStats.SetActive(true);

        ShopKeeper.TriggerShopStateEvent(shopManager, false);
    }
}