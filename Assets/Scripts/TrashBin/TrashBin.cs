using UnityEngine;

public class TrashBin : BaseStation
{
    public override bool CanInteract(PlayerStatusController player)
    {
        return player != null && player.IsHoldingSomething();
    }

    public override void Interact(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        if (IsServer)
        {
            Ingredient heldIngredient = player.GetIngredient();
            if (heldIngredient != null)
            {
                ICommand discard = new DiscardIngredientCommand(heldIngredient);
                discard.Execute();
                player.ClearIngredient();
                TriggerInteract();
                return;
            }

            Order heldOrder = player.GetOrder();
            if (heldOrder != null)
            {
                ICommand discard = new DiscardOrderCommand(heldOrder);
                discard.Execute();
                player.ClearOrder();
                TriggerInteract();
                return;
            }
        }
        else
        {
            TriggerInteract();
        }
    }
}
