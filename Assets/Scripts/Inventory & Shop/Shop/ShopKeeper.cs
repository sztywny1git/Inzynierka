using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopKeeper : MonoBehaviour
{
    public Animator anim;
    public CanvasGroup shopCanvasGroup;
    public ShopManager shopManager;

    public CanvasGroup UltimateEqCanvas;
    private bool UltimateEqPanel = true;
    //public GameObject UltimateShop;
    public GameObject EqPanel;
    public GameObject StatsPanel;

    public GameObject hotbar;
    public GameObject visibleStats;

    [SerializeField] private List<ShopItems> shopItems;

    public static event Action<ShopManager, bool> OnShopStateChanged;
    private bool playerInRange;

    public static bool isShopOpen;//wspolna zmienna dla calej gry a nie inna dla kazdej instancji obiektu

    public AudioSource audioSource;
    public AudioClip shopOpenSound;



    void Update()
    {
        if (playerInRange && Input.GetButtonDown("OpenShop"))
        {
            if (!isShopOpen)
            {
                Time.timeScale = 0; // Pause the game

                audioSource.PlayOneShot(shopOpenSound);

                //UltimateShop.SetActive(true);
                EqPanel.SetActive(false);
                StatsPanel.SetActive(false);
                hotbar.SetActive(false);
                visibleStats.SetActive(false);

                UltimateEqCanvas.alpha = 1;
                UltimateEqCanvas.blocksRaycasts = true;


                isShopOpen = true;
                OnShopStateChanged?.Invoke(shopManager, true);
                shopCanvasGroup.alpha = 1;
                shopCanvasGroup.blocksRaycasts = true;
                shopCanvasGroup.interactable = true;
                OpenItemShop();
            }
            else
            {
                Time.timeScale = 1; // unPause the game

                audioSource.Stop();

                isShopOpen = false;
                OnShopStateChanged?.Invoke(shopManager, false);
                shopCanvasGroup.alpha = 0;
                shopCanvasGroup.blocksRaycasts = false;
                shopCanvasGroup.interactable = false;

                UltimateEqCanvas.alpha = 0;
                UltimateEqCanvas.blocksRaycasts = false;
                UltimateEqPanel = false;

                EqPanel.SetActive(true);
                StatsPanel.SetActive(true);
                hotbar.SetActive(true);
                visibleStats.SetActive(true);


            }
        }
    }

    public void OpenItemShop()
    {
        shopManager.PopulateShopItems(shopItems);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("playerInRange", true);
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("playerInRange", false);
            playerInRange = false;
        }
    }

}
