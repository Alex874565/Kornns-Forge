public interface IOrderFactory
{
    OrderProgress CreateOrder(OrderData order, float timer, int points);
}
