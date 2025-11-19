using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;

    public GameObject player;
    public Text healthText;
    public Slider healthSlider;

    public float health;
    public float maxHealth;

    public int coins;
    public int gems;

    public Text coinsValue;
    public Text gemsValue;


    void Awake()
    {
        if(playerStats != null)
        {
            Destroy(playerStats);
            
        }
        else
        {
            playerStats = this;
        }
        
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        health = maxHealth;
        SetHealthUI();
    }

    public void DealDamage(float damage)
    {
        health -= damage;
        CheckDeath();
        SetHealthUI();
    }

    public void HealCharacter(float heal)
    {
        health += heal;
        CheckOverheal();
        SetHealthUI();
    }

    private void SetHealthUI()
    {
        healthSlider.value = CalculateHealthPercentage();
        healthText.text = Mathf.Ceil(health).ToString() + " / " + Mathf.Ceil(maxHealth).ToString();
    }

    private void CheckOverheal()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    private void CheckDeath()
    {
        if (health <= 0)
        {
            Destroy(player);
        }
    }

    private float CalculateHealthPercentage()
    {
        return health/maxHealth;
    }

    public void AddCurrency(CurrencyPickup currency)
    {
        if(currency.currentObject == CurrencyPickup.pickupObject.COIN)
        {
            coins += currency.pickupQuantity;
            coinsValue.text = "Gold: " + coins.ToString();
        }
        else if(currency.currentObject == CurrencyPickup.pickupObject.GEM)
        {
            gems += currency.pickupQuantity;
            coinsValue.text = "Gems: " + gems.ToString();
        }
    }


}
