using UnityEngine;

[CreateAssetMenu(fileName = "New Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite icon;
    public bool isGold;
    public int stackSize = 3;

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

}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}


public enum ItemType
{
    Consumable,
    Collectible,
    Ring,
    Weapon,   
    Helmet,
    Chestplate,
    Legs,
    Boots      
}