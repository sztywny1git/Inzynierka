using UnityEngine;

public class UseItem : MonoBehaviour
{
    private StatMediator currentMediator;

    public void RegisterPlayer(StatMediator mediator)
    {
        currentMediator = mediator;
    }

    public void UnregisterPlayer()
    {
        currentMediator = null;
    }

    public void ApplyItemEffects(ItemSO itemSO)
    {
        if (itemSO.itemType != ItemType.Consumable)
            return;

        if (currentMediator != null)
        {
            currentMediator.HandleConsumable(itemSO);
        }
    }

    public void RemoveItemEffects(string source)
    {

    }
}