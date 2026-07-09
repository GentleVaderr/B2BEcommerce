using Business.Abstract;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserDal _userDal;

        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public List<User> GetAll()
        {
            return _userDal.GetAll();
        }

        public User? GetById(int id)
        {
            return _userDal.Get(u => u.Id == id);
        }

        public void Add(User user)
        {
            _userDal.Add(user);
        }

        public void Delete(User user)
        {
            _userDal.Delete(user);
        }

        public void Update(User user)
        {
            _userDal.Update(user);
        }
    }
}
