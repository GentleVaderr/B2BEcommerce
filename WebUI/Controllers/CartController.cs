using Azure;
using Business.Abstract;
using Entities.Concrete;
using Entities.Concrete.FrontEnd;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;

namespace WebUI.Controllers
{
    [CustomerAuth]
    public class CartController : Controller
    {
        private readonly ICartItemService _cartItemService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IGoogleAnalyticsService _gaService;

        public CartController(ICartItemService cartItemService, IOrderService orderService,
            IOrderDetailService orderDetailService, IProductService productService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IGoogleAnalyticsService gaService)
        {
            _cartItemService = cartItemService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _productService = productService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _gaService = gaService;
        }

        // 1. SEPETİ GÖRÜNTÜLEME
        public async Task<IActionResult> Index()
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
            var customerOrders = _orderService.GetAll()
                                              .Where(o => o.UserId == currentUserId)
                                              .OrderByDescending(o => o.OrderDate)
                                              .ToList();
            ViewBag.MyOrders = customerOrders;
            
            await _gaService.TrackEventAsync("view_cart", new 
            { 
                debug_mode = 1,
                currency = "TRY",
                cart_total_amount = cartDto.CartTotalAmount,
                items = cartDto.CartItems.Select((item, i) => new
                {
                    item_id = item.ProductId.ToString(),
                    item_name = item.ProductName,
                    price = item.Price,
                    quantity = item.Quantity,
                    index = i,

                    //Filler
                    excel_basket = "",
                    affiliation = "",
                    coupon = "",
                    discount = "",
                    item_brand = "",
                    item_category = "",
                    item_category2 = "",
                    item_category3 = "",
                    item_list_id = "",
                    item_list_name = "",
                    item_variant = "",
                    kdv_price = 0.0m,
                    in_stock = false,
                    stock_limit = "",
                    order_limit = "",
                }).ToArray()
            }, HttpContext, currentUserId.ToString());

            return View(cartDto);
        }

        // 2. SEPETE ÜRÜN EKLEME 
        public async Task<IActionResult> AddToCart(int productId)
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

            await _gaService.TrackEventAsync("add_to_cart", new 
            {
                debug_mode = 1,
                currency = "TRY",
                price = product.Price,
                items = new[]
                {
                    new
                    {
                        item_id = product.Id.ToString(),
                        item_name = product.Name,
                        price = product.Price,
                        quantity = 1, 
                        index = 0, 

                        // Filler
                        affiliation = "",
                        coupon = "",
                        discount = 0.0m, 
                        item_brand = "",
                        item_category = "",
                        item_category2 = "",
                        item_category3 = "",
                        item_list_id = "",
                        item_list_name = "",
                        item_variant = "",
                        kdv_price = 0.0m, 
                        in_stock = false, 
                        stock_limit = "", 
                        order_limit = ""  
                    }
                }
            }, HttpContext, currentUserId.ToString());

            return RedirectToAction("Index");
        }

        // 3. SİPARİŞİ ONAYLAMA
        [HttpPost]
        public async Task<IActionResult> CompleteOrder()
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
                        UnitPrice = product.Price,

                        Quantity = item.Quantity
                    };
                    _orderDetailService.Add(orderDetail);
                }

                _cartItemService.Delete(item);
            }


            await _gaService.TrackEventAsync("purchase", new
            {
                debug_mode = 1,
                transaction_id = newOrder.Id.ToString(), 
                currency = "TRY",
                value = totalOrderPrice, 

                kdv_exclude_value = 0.0m,
                coupon = "",
                sending_date = "",
                routine_order = false, 
                sas_no = "",
                shipping_tier = "",
                items = cartItems.Select((item, i) =>
                {
                    var product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);

                    return new
                    {
                        item_id = item.ProductId.ToString(),
                        item_name = product != null ? product.Name : "Bilinmeyen Ürün",
                        price = product != null ? product.Price : 0,
                        quantity = item.Quantity,
                        index = i,

                        // Sonradan Doldurulacak Ürün Parametreleri
                        excel_basket = "",
                        affiliation = "",
                        coupon = "",
                        discount = 0.0m,
                        item_brand = "",
                        item_category = "",
                        item_category2 = "",
                        item_category3 = "",
                        item_list_id = "",
                        item_list_name = "",
                        item_variant = "",
                        kdv_price = 0.0m,
                        in_stock = false,
                        stock_limit = "",
                        order_limit = ""
                    };
                }).ToArray()
            }, HttpContext, currentUserId.ToString());

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