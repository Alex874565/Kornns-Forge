using System;
using Unity.Netcode;
using UnityEngine;

public class MaterialSpawner : BaseStation
{
    public event EventHandler OnPlayerGrabbedMaterial;

    [SerializeField] private IngredientSO ingredientSO;
    [Header("Tiredness")]
    [SerializeField] private float energy = 2f;

    public override bool CanInteract(PlayerStatusController player)
    {
        Debug.Log("Checking if player can interact with material spawner");
        Debug.Log($"Player is holding something: {player != null && player.IsHoldingSomething()}");
        return player != null && !player.IsHoldingSomething();
    }

    public override void Interact(PlayerStatusController player)
    {
        Debug.Log("Interacted with material spawner");

        if (player == null) return;

        if (!IsServer)
        {
            SpawnIngredientServerRpc(player.NetworkObjectId);
            return;
        }

        SpawnIngredientForPlayer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnIngredientServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                playerNetworkObjectId,
                out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player =
            playerNetworkObject.GetComponent<PlayerStatusController>();

        SpawnIngredientForPlayer(player);
    }

    private void SpawnIngredientForPlayer(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        TriggerInteract();

        Ingredient.SpawnIngredient(ingredientSO, player);

        // consume a small amount of player's energy when grabbing material
        if (player != null && IsServer)
            player.GetTired(this.energy);

        OnPlayerGrabbedMaterial?.Invoke(this, EventArgs.Empty);
    }
}