using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class OrderSummaryDto
    {
        public int Id {  get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
    }
}
