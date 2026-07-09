using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IOrderService
    {
        List<Order> GetAll();
        Order? GetById(int id);
        void Add(Order order);
        void Update(Order order);
        void Delete(Order order);
    }
}
