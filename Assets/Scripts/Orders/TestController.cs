using Unity.Netcode;
using UnityEngine;

public class TestController : NetworkBehaviour
{
    public OrderManager orderManager;
    public OrderData[] testOrders;

    [Header("Test Ingredients")]
    public IngredientSO meltedGold;
    public IngredientSO straightenedGold;
    public IngredientSO choppedWood;
    public IngredientSO meltedIron;
    public IngredientSO straightenedIron;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Debug.Log("O → Spawn Random Order");
        Debug.Log("1 → Add Melted Gold");
        Debug.Log("2 → Add Straightened Gold");
        Debug.Log("3 → Add Chopped Wood");
        Debug.Log("Q → Add Melted Iron");
        Debug.Log("W → Add Straightened Iron");
        Debug.Log("G → Craft Order");
        Debug.Log("SPACE → Deliver Order");
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.O))
            SpawnRandomOrderServerRpc();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            orderManager.AddItem(meltedGold);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            orderManager.AddItem(straightenedGold);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            orderManager.AddItem(choppedWood);

        if (Input.GetKeyDown(KeyCode.Q))
            orderManager.AddItem(meltedIron);

        if (Input.GetKeyDown(KeyCode.W))
            orderManager.AddItem(straightenedIron);

        if (Input.GetKeyDown(KeyCode.G))
            orderManager.TryCraft();

        if (Input.GetKeyDown(KeyCode.Space))
            orderManager.TryDeliver();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnRandomOrderServerRpc()
    {
        if (testOrders == null || testOrders.Length == 0)
            return;

        int index = Random.Range(0, testOrders.Length);
        SpawnRandomOrderClientRpc(index);
    }

    [ClientRpc]
    private void SpawnRandomOrderClientRpc(int index)
    {
        if (index < 0 || index >= testOrders.Length)
            return;

        OrderData order = testOrders[index];
        orderManager.AddOrder(order);
    }
}