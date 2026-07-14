using Core.DataAccess;
using Entities.Concrete.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Abstract
{
    public interface ICartItemDal : IEntityRepository<CartItem>
    {
    }
}
