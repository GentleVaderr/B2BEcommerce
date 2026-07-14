using Entities.Concrete;
using Entities.Concrete.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface ICartItemService
    {
        List<CartItem> GetAll();
        CartItem? GetById(int id);
        void Update(CartItem cartItem);
        void Delete(CartItem cartItem);
        void Add(CartItem cartItem);
    }
}
