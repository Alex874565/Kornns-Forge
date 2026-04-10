using UnityEngine;

public class TestController : MonoBehaviour
{
    public OrderManager orderManager;
    public OrderData[] testOrders;

    private void Start()
    {
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
            SpawnRandomOrder();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Gold, MaterialProcess.Melt);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Gold, MaterialProcess.Straighten);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Wood, MaterialProcess.Chop);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Iron, MaterialProcess.Melt);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            orderManager.AddItem(ItemType.Material, MaterialType.Iron, MaterialProcess.Straighten);
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

    void SpawnRandomOrder()
    {
        int index = Random.Range(0, testOrders.Length);
        OrderData order = testOrders[index];

        orderManager.AddOrder(order);
    }
}