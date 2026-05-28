using UnityEngine;

public class OrderFactory : IOrderFactory
{
    public OrderProgress CreateOrder(OrderData order, float timer, int points, float timeRemaining)
    {
        if(order == null)
        {
            Debug.LogError("OrderData is null");
            return null;
        }

        return new OrderProgress(order, timer, points, timeRemaining);
    }
}
