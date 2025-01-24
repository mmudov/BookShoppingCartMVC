using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMVC.Models.DTOs
{
    public class StockDTO
    {
        public int BookId { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a not negative")]
        public int Quantity { get; set; }
    }
}
