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

    [SerializeField] private List<ShopItems> shopItems;

    public static event Action<ShopManager, bool> OnShopStateChanged;
    private bool playerInRange;
    private bool isShopOpen;



    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))//Input.GetButtonDown("Interact") ale trzeba ustawic w edit i project settings
        {
            if (!isShopOpen)
            {
                Time.timeScale = 0; // Pause the game
                isShopOpen = true;
                OnShopStateChanged?.Invoke(shopManager, true);
                shopCanvasGroup.alpha = 1;
                shopCanvasGroup.blocksRaycasts = true;
                shopCanvasGroup.interactable = true;
                OpenItemShop();
            }
            else
            {
                Time.timeScale = 1; // Pause the game
                isShopOpen = false;
                OnShopStateChanged?.Invoke(shopManager, false);
                shopCanvasGroup.alpha = 0;
                shopCanvasGroup.blocksRaycasts = false;
                shopCanvasGroup.interactable = false;
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
