namespace BookShoppingCartMVC.Repositories;

public interface IUserOrderRepository
{
    Task<IEnumerable<Order>> UserOreders(bool getAll = false);
    Task ChangeOrderStatus(UpdateOrderStatusModel data);
    Task TogglePaymentStatus(int orderid);
    Task<Order?> GetOrderById(int id);
    Task<IEnumerable<OrderStatus>> GetOrderStatuses();
}
