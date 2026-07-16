using Entities.Concrete.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class CartDto
    {
        public List<CartItemDto>? CartItems { get; set; }
        public decimal CartTotalAmount { get; set; }
    }
}
