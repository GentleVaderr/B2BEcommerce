using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class Order : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Beklemede";

        //Bağlantılar
        public User User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
