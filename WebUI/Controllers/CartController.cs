using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models;

namespace WebUI.Controllers
{
    [CustomerAuth]
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public CartController(IProductService productService, IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        // 1. SEPET VE SİPARİŞLERİ GÖRÜNTÜLEME
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItems>>("Cart") ?? new List<CartItems>();

            // Hafızadaki kullanıcı ID'sini alıyoruz. Eğer yoksa (giriş yapmamışsa) 0 dönecek.
            int currentUserId = HttpContext.Session.GetInt32("CurrentUserId") ?? 0;

            // Güvenlik: Eğer kullanıcı giriş yapmamışsa doğrudan Login sayfasına at!
            if (currentUserId == 0) return RedirectToAction("Login", "Auth");

            // Artık kullanıcının kendi siparişlerini getiriyoruz
            ViewBag.MyOrders = _orderService.GetAll().Where(o => o.UserId == currentUserId).ToList();

            return View(cart);
        }

        // 2. SEPETE ÜRÜN EKLEME
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _productService.GetAll().FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetJson<List<CartItems>>("Cart") ?? new List<CartItems>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity; // Ürün zaten varsa adedini artır
            }
            else
            {
                cart.Add(new CartItems // Yoksa yeni ürün olarak ekle
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetJson("Cart", cart); // Güncel sepeti tekrar Session'a kaydet
            return RedirectToAction("Index", "Product"); // Ürünler sayfasına geri dön
        }

        // 3. SEPETTEN ÜRÜN ÇIKARMA
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetJson<List<CartItems>>("Cart") ?? new List<CartItems>();
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetJson("Cart", cart);
            }

            return RedirectToAction("Index"); // Sepet sayfasına geri dön
        }

        // 4. SİPARİŞİ ONAYLAMA VE VERİTABANINA KAYDETME
        [HttpPost]
        public IActionResult CompleteOrder()
        {
            var cart = HttpContext.Session.GetJson<List<CartItems>>("Cart") ?? new List<CartItems>();
            if (cart.Count == 0) return RedirectToAction("Index");

            int currentUserId = HttpContext.Session.GetInt32("CurrentUserId") ?? 0;
            if (currentUserId == 0) return RedirectToAction("Login", "Auth");

            // 1. Ana Siparişi Kaydet
            var newOrder = new Entities.Concrete.Order
            {
                UserId = currentUserId,
                OrderDate = DateTime.Now,
                Status = "Bekliyor",
                TotalPrice = cart.Sum(c => c.TotalPrice)
            };
            _orderService.Add(newOrder);

            // 2. Sepetteki Ürünleri OrderDetail Olarak Kaydet
            foreach (var item in cart)
            {
                var orderDetail = new Entities.Concrete.OrderDetail
                {
                    OrderId = newOrder.Id, // Ana siparişin ID'si ile birbirine bağlıyoruz
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity
                };
                _orderDetailService.Add(orderDetail);
            }

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }
    }
}