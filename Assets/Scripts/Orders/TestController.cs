using Unity.Netcode;
using UnityEngine;

public class TestController : NetworkBehaviour
{
    public OrderManager orderManager;
    public OrderData[] testOrders;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Debug.Log("E → Spawn Random Order");

        Debug.Log("1 → Melt Gold");
        Debug.Log("2 → Straighten Gold");
        Debug.Log("3 → Chop Wood");

        Debug.Log("Q → Melt Iron");
        Debug.Log("W → Straighten Iron");

        Debug.Log("SPACE → Deliver Order");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"CLIENT pressed E. IsClient={IsClient} IsOwner={IsOwner} IsServer={IsServer} OwnerClientId={OwnerClientId}");
            SpawnRandomOrderServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Gold, Process.Melt);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Gold, Process.Straighten);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Wood, Process.Chop);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Iron, Process.Melt);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Iron, Process.Straighten);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            orderManager.TryCraft();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            orderManager.TryDeliver();
        }
    }

    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SpawnRandomOrderServerRpc()
    {
        int index = Random.Range(0, testOrders.Length);

        SpawnRandomOrderClientRpc(index);
    }
    
    
    [ClientRpc]
    void SpawnRandomOrderClientRpc(int index)
    {
        Debug.Log($"CLIENT received order index {index}. IsOwner={IsOwner} IsClient={IsClient} IsServer={IsServer}");
        OrderData order = testOrders[index];
        
        orderManager.AddOrder(order);
    }
}