using UnityEngine;

public class OrdersUIManager : MonoBehaviour
{
    public OrderManager orderManager;
    public Transform ordersPanel;
    public GameObject orderPrefab;

    private void OnEnable()
    {
        if (orderManager != null)
            orderManager.OnOrdersUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        if (orderManager != null)
            orderManager.OnOrdersUpdated -= RefreshUI;
    }

    public void RefreshUI()
    {
        foreach (Transform child in ordersPanel)
            Destroy(child.gameObject);

        if (orderManager == null) return;

        foreach (var progress in orderManager.activeOrders)
        {
            GameObject obj = Instantiate(orderPrefab, ordersPanel);
            obj.GetComponent<OrderUI>().Setup(progress);
        }
    }
}