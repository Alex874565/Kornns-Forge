using UnityEngine;

public class TestController : MonoBehaviour
{
    public OrderManager orderManager;
    public OrderData[] testOrders;

    private void Start()
    {
        Debug.Log("E → Spawn Random Order");

        Debug.Log("1 → Forge");
        Debug.Log("2 → Melt");
        Debug.Log("3 → Sharpen");

        Debug.Log("Q → Sword (Iron)");
        Debug.Log("W → Sword (Gold)");

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
            orderManager.AddStep(CraftingSteps.Forge);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            orderManager.AddStep(CraftingSteps.Melt);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            orderManager.AddStep(CraftingSteps.Sharpen);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            orderManager.AddItem(ItemType.Sword, MaterialType.Iron);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            orderManager.AddItem(ItemType.Sword, MaterialType.Gold);
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