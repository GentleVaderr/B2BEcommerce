using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class OrderItem : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        //Bağlantılar
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
