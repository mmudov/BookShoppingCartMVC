namespace BookShoppingCartMVC.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOreders();
    }
}