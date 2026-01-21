using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;
    public SpriteRenderer sr;
    public Animator anim;

    public bool canBePickedUp = true;
    private bool pickedUp = false;

    public int quantity;
    public static event Action<ItemSO, int> OnItemLooted;

    public AudioSource audioSource;
    public AudioClip goldPickupSound;
    public AudioClip itemPickupSound;


    private void OnValidate()
    {
        if (itemSO == null)
            return;

        UpdateAppearance();
    }

    public void Initialize(ItemSO itemSO, int quantity)
    {
        this.itemSO = itemSO;
        this.quantity = quantity;
        //canBePickedUp = false;
        UpdateAppearance();
        StartCoroutine(EnablePickupAfterDelay());
    }

    private IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        canBePickedUp = true;
    }

    private void UpdateAppearance()
    {
        sr.sprite = itemSO.icon;
        this.name = itemSO.itemName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickedUp) return;

        if (collision.CompareTag("Player") && canBePickedUp == true)
        {
            pickedUp = true;
            canBePickedUp = false;
            GetComponent<Collider2D>().enabled = false;

            anim.Play("LootPickup");

            if (itemSO.isGold)
            {
                audioSource.PlayOneShot(goldPickupSound);
            }
            else
            {
                audioSource.PlayOneShot(itemPickupSound);
            }

            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject, .5f);
        }
    }

    /*private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBePickedUp = true;
        }
    }*/
}
