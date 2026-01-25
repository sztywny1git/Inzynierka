using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite icon;
    public bool isGold;
    public int stackSize = 3;

    [Header("Drop Quantity Settings")]
    public int minDropAmount = 1;
    public int maxDropAmount = 1;

    [Header("Item Type")]
    public ItemType itemType = ItemType.Consumable;

    [Header("Stats")]
    public int currentHearts;
    public int Resource;
    public int armor;
    public float speed;
    public int damage;
    public float fireRate;
    public float CriticalChance;
    public float CriticalDamage;

    [Header("For Temporary Items")]
    public float duration;

    [Header("Value")]
    public int value;

    [Header("Rarity")]
    public Rarity rarity;

    public ItemSO CreateRandomInstance()
    {
        if (isGold || itemType == ItemType.Collectible || stackSize > 1)
        {
            return this;
        }

        ItemSO clone = Instantiate(this);
        clone.name = this.name; 

        float multiplier = UnityEngine.Random.Range(0.8f, 1.3f);

        clone.currentHearts = Mathf.RoundToInt(this.currentHearts * multiplier);
        clone.Resource = Mathf.RoundToInt(this.Resource * multiplier);
        clone.armor = Mathf.RoundToInt(this.armor * multiplier);
        clone.damage = Mathf.RoundToInt(this.damage * multiplier);
        clone.value = Mathf.RoundToInt(this.value * multiplier);

        clone.speed = (float)Math.Round(this.speed * multiplier, 2);
        clone.fireRate = (float)Math.Round(this.fireRate * multiplier, 2);
        clone.CriticalChance = (float)Math.Round(this.CriticalChance * multiplier, 2);
        clone.CriticalDamage = (float)Math.Round(this.CriticalDamage * multiplier, 2);
        
        return clone;
    }
}