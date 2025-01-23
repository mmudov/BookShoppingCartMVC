using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMVC.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccesor;
        public UserOrderRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccesor, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccesor = httpContextAccesor;
        }

        public async Task<IEnumerable<Order>> UserOreders()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged in");

            var orders = _db.Orders
                .Include(x => x.OrderStatus)
                .Include(x => x.OrderDetail)
                .ThenInclude(x => x.Book)
                .ThenInclude(x => x.Genre)
                .Where(x => x.UserId == userId)
                .AsQueryable();

            return orders;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccesor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
