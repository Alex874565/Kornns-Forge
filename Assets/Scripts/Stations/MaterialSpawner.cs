using System;
using Unity.Netcode;
using UnityEngine;

public class MaterialSpawner : BaseStation
{
    public event EventHandler OnPlayerGrabbedMaterial;

    [SerializeField] private IngredientSO ingredientSO;

    public override bool CanInteract(PlayerStatusController player)
    {
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

        Ingredient.SpawnIngredient(ingredientSO, player);

        OnPlayerGrabbedMaterial?.Invoke(this, EventArgs.Empty);
    }
}