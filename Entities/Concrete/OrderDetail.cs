using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class OrderDetail : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; } // Hangi siparişe ait?
        public int ProductId { get; set; } // Hangi ürün?
        public decimal UnitPrice { get; set; } // O anki fiyatı
        public int Quantity { get; set; } // Kaç adet alındı?
        
        //bağlama
        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
    }
}
