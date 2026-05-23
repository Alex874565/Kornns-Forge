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

        Ingredient heldIngredient = player.GetIngredient();
        if (heldIngredient == null) return;

        if (IsServer)
        {
            ICommand discard = new DiscardIngredientCommand(heldIngredient);
            discard.Execute();
            player.ClearIngredient();
        }

        TriggerInteract();
    }
}
