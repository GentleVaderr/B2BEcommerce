namespace WebUI.Models
{
    public class CartItems
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        // Ürün fiyatı ile adedinin çarpımı
        public decimal TotalPrice => Price * Quantity;
    }
}