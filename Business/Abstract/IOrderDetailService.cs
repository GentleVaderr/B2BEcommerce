using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IOrderDetailService
    {
        List<OrderDetail> GetAll();
        OrderDetail? GetById(int id);
        void Update(OrderDetail orderDetail);
        void Delete(OrderDetail orderDetail);
        void Add(OrderDetail orderDetail);
    }
}
