using Business.Abstract;
using Entities.Concrete;
using Entities.Concrete.FrontEnd;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using Entities.DTOs;

namespace WebUI.Controllers
{
    [CustomerAuth]
    public class CartController : Controller
    {
        private readonly ICartItemService _cartItemService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;

        public CartController(ICartItemService cartItemService, IOrderService orderService, IOrderDetailService orderDetailService, IProductService productService)
        {
            _cartItemService = cartItemService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

        // 1. SEPETİ GÖRÜNTÜLEME
        public IActionResult Index()
        {
            int currentUserId = HttpContext.Session.GetInt32("CurrentUserId") ?? 0;
            var userCartItems = _cartItemService.GetAll().Where(c => c.UserId == currentUserId).ToList();

            var cartDto = new CartDto();
            cartDto.CartItems = new List<CartItemDto>();

            var allProducts = _productService.GetAll();

            foreach (var item in userCartItems)
            {
                var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    cartDto.CartItems.Add(new CartItemDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price, 
                        Quantity = item.Quantity,
                        ImageUrl = product.ImageUrl
                    });
                }
            }

            cartDto.CartTotalAmount = cartDto.CartItems.Sum(c => c.Price * c.Quantity);

            return View(cartDto);
        }

        // 2. SEPETE ÜRÜN EKLEME 
        public IActionResult AddToCart(int productId)
        {
            int currentUserId = HttpContext.Session.GetInt32("CurrentUserId") ?? 0;

            var product = _productService.GetAll().FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return NotFound(); 
            }

            var existingItem = _cartItemService.GetAll()
                                .FirstOrDefault(c => c.UserId == currentUserId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
                _cartItemService.Update(existingItem);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    UserId = currentUserId,
                    ProductId = productId,
                    Quantity = 1
                };
                _cartItemService.Add(newCartItem);
            }

            return RedirectToAction("Index");
        }

        // 3. SİPARİŞİ ONAYLAMA
        [HttpPost]
        public IActionResult CompleteOrder()
        {
            int currentUserId = HttpContext.Session.GetInt32("CurrentUserId") ?? 0;
            var cartItems = _cartItemService.GetAll().Where(c => c.UserId == currentUserId).ToList();
            if (cartItems.Count == 0) return RedirectToAction("Index");

            var allProducts = _productService.GetAll();

            decimal totalOrderPrice = 0;
            foreach (var item in cartItems)
            {
                var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    totalOrderPrice += (product.Price * item.Quantity);
                }
            }

            var newOrder = new Order
            {
                UserId = currentUserId,
                OrderDate = DateTime.Now,
                Status = "Bekliyor",
                TotalPrice = totalOrderPrice
            };
            _orderService.Add(newOrder);

            foreach (var item in cartItems)
            {
                var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.Price,

                        Quantity = item.Quantity
                    };
                    _orderDetailService.Add(orderDetail);
                }

                _cartItemService.Delete(item);
            }

            return RedirectToAction("Index");
        }

        // 4. SEPETTEN ÜRÜN SİLME
        public IActionResult RemoveFromCart(int id)
        {
            var item = _cartItemService.GetAll().FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                _cartItemService.Delete(item);
            }
            return RedirectToAction("Index");
        }
    }
}