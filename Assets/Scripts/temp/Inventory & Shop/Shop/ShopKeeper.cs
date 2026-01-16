using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopKeeper : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip shopOpenSound;

    [SerializeField] private List<ShopItems> shopItems;

    private bool playerInRange;
    public static bool isShopOpen;

    public static event Action<ShopManager, bool> OnShopStateChanged;

    public static void TriggerShopStateEvent(ShopManager manager, bool isOpen)
    {
        OnShopStateChanged?.Invoke(manager, isOpen);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.ToggleShop(shopItems, audioSource, shopOpenSound);
            }
        }
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
            
            if (isShopOpen && ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.CloseShop();
            }
        }
    }
}