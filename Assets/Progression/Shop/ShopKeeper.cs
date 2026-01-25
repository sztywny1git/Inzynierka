using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopKeeper : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip shopOpenSound;

    [Header("Shop Generation")]
    [SerializeField] private LootTableSO lootTable;
    [SerializeField] private int itemsCount = 5;

    private List<ShopItems> shopItems = new List<ShopItems>();

    private bool playerInRange;
    public static bool isShopOpen;

    public static event Action<ShopManager, bool> OnShopStateChanged;

    public static void TriggerShopStateEvent(ShopManager manager, bool isOpen)
    {
        OnShopStateChanged?.Invoke(manager, isOpen);
    }

    private void Start()
    {
        GenerateShopItems();
    }

    private void GenerateShopItems()
    {
        shopItems.Clear();

        if (lootTable == null) return;

        for (int i = 0; i < itemsCount; i++)
        {
            ItemSO randomItem = lootTable.GetRandomItem();

            if (randomItem != null)
            {
                ItemSO instancedItem = randomItem.CreateRandomInstance();

                ShopItems newShopEntry = new ShopItems();
                newShopEntry.itemSO = instancedItem;
                newShopEntry.price = instancedItem.value;

                shopItems.Add(newShopEntry);
            }
        }
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