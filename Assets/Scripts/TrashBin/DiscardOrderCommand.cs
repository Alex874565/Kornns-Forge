using UnityEngine;

public class DiscardOrderCommand : ICommand
{
    private Order order;

    public DiscardOrderCommand(Order order)
    {
        this.order = order;
    }

    public void Execute()
    {
        if (order != null)
        {
            order.DestroySelf();
        }
    }
}
