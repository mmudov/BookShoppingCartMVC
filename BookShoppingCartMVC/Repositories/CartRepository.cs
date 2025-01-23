using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMVC.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccesor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccesor, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccesor = httpContextAccesor;
        }

        public async Task<int> AddItem(int bookId, int quantity)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };

                    _db.ShoppingCarts.Add(cart);
                }

                _db.SaveChanges();

                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    var book = _db.Books.Find(bookId);

                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = quantity,
                        UnitPrice = book.Price
                    };

                    _db.CartDetails.Add(cartItem);
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
            }

            var cartItemCount = await GetCartItemCount(userId);

            return cartItemCount;
        }

        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetCart(userId);

                if (cart is null)
                {
                    throw new Exception("Cart is empty");
                }

                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is null)
                {
                    throw new Exception("No items in the cart");
                }
                else if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }

                _db.SaveChanges();
                //transaction.Commit();
            }
            catch (Exception ex)
            {
            }

            var cartItemCount = await GetCartItemCount(userId);

            return cartItemCount;
        }

        public async Task <ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();

            if (userId == null)
                throw new Exception("Invalid user id");

            var shoppingCart = await _db.ShoppingCarts
                .Include(a => a.CartDetails)
                .ThenInclude(a => a.Book)
                .ThenInclude(a => a.Genre)
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            return shoppingCart;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if(!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await
                (
                from cart in _db.ShoppingCarts
                join cartDetail in _db.CartDetails
                on cart.Id equals cartDetail.ShoppingCartId
                select new { cartDetail.Id }
                ).ToListAsync();

            return data.Count;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);

            return cart;
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();

            try 
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                var cart = await GetCart(userId);

                if (cart is null)
                    throw new Exception("Invalid cart");

                var cartDedatil = _db.CartDetails.Where(a => a.ShoppingCartId == cart.Id).ToList();

                if (cartDedatil.Count == 0)
                    throw new Exception("Cart is empty");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = "model.Name",
                    Email = "model.Email",
                    MobileNumber = "model.MobileNumber",
                    Address = "model.Address",
                    PaymentMethod = "model.PaymentMethod",
                    OrderStatusId = 1 // Pending
                };

                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach(var item in cartDedatil)
                {
                    var orderDedatil = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };

                    _db.OrderDetails.Add(orderDedatil);
                }

                _db.SaveChanges();

                //Removing the cartdetails
                _db.CartDetails.RemoveRange(cartDedatil);
                _db.SaveChanges();

                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccesor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);

            return userId;
        }
    }
}
