using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO; 
    public SpriteRenderer sr;
    public Animator anim;

    public bool canBePickedUp = false;
    private bool pickedUp = false;

    public int quantity;
    public static event Action<ItemSO, int> OnItemLooted;

    public AudioSource audioSource;
    public AudioClip goldPickupSound;
    public AudioClip itemPickupSound;

    public void Initialize(ItemSO item, int quantity)
    {
        this.itemSO = item;
        this.quantity = quantity;
        this.canBePickedUp = false;

        if (sr != null)
        {
            sr.sortingLayerName = "Decor";
            sr.sortingOrder = 10;
        }

        UpdateAppearance();
        StartCoroutine(EnablePickupAfterDelay());
    }

    private IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(0.7f);
        canBePickedUp = true;
    }

    private void UpdateAppearance()
    {
        if(itemSO != null)
        {
            sr.sprite = itemSO.icon;
            this.name = itemSO.itemName;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickedUp) return;

        if (collision.CompareTag("Player") && canBePickedUp == true)
        {
            pickedUp = true;
            canBePickedUp = false;
            GetComponent<Collider2D>().enabled = false;

            if(anim != null) anim.Play("LootPickup");

            if (itemSO.isGold)
            {
                if(audioSource && goldPickupSound) audioSource.PlayOneShot(goldPickupSound);
            }
            else
            {
                if(audioSource && itemPickupSound) audioSource.PlayOneShot(itemPickupSound);
            }

            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject, .5f);
        }
    }
}