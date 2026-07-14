using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebAdmin.Controllers
{
    
    public class AuthController : Controller
    {
        public IActionResult AdminTransfer(string fullName, string role)
        {
            if (role == "Admin")
            {
                HttpContext.Session.SetString("CurrentUserRole", role);
                HttpContext.Session.SetString("CurrentUserFullName", fullName);
                
                return RedirectToAction("Index", "Order");
            }
            return Redirect("https://localhost:7089/Auth/Login");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Redirect("https://localhost:7089/Auth/Login");
        }
    }
}