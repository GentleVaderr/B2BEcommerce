using Business.Abstract;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Concrete.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class CartItemManager : ICartItemService
    {
        private readonly ICartItemDal _cartItemDal;
        public CartItemManager(ICartItemDal cartItemDal)
        {
            _cartItemDal = cartItemDal;
        }

        public void Add(CartItem cartItem)
        {
            _cartItemDal.Add(cartItem);
        }

        public void Delete(CartItem cartItem)
        {
            _cartItemDal.Delete(cartItem);
        }

        public List<CartItem> GetAll()
        {
            return _cartItemDal.GetAll();
        }

        public CartItem? GetById(int id)
        {
            return _cartItemDal.Get(ci => ci.Id == id);
        }

        public void Update(CartItem cartItem)
        {
            _cartItemDal.Update(cartItem);
        }
    }
}
