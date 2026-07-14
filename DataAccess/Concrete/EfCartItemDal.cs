using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFranework;
using Entities.Concrete.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete
{
    public class EfCartItemDal : EfEntityRepositoryBase<CartItem, AppDbContext>, ICartItemDal
    {
    }
}
