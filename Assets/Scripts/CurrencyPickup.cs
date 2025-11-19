using UnityEngine;

public class CurrencyPickup : MonoBehaviour
{
    public enum pickupObject{COIN,GEM};
    public pickupObject currentObject;
    public int pickupQuantity;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        print(other);
        if(other.name == "Player")
        {
            PlayerStats.playerStats.AddCurrency(this);
            Destroy(gameObject);
            
        }
    }
}
