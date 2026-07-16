using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; } 
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
