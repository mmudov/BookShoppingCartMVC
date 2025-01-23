using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMVC.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepo;

        public UserOrderController(IUserOrderRepository userOrderRepo)
        {
            _userOrderRepo = userOrderRepo;
        }

        public IUserOrderRepository UserOrderRepo { get; }

        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepo.UserOreders();

            return View(orders);
        }
    }
}
