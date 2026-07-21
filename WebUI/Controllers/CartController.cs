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

        public CartController(ICartItemService cartItemService, IOrderService orderService, IOrderDetailService orderDetailService, IProductService productService, IHttpClientFactory httpClientFactory)
        {
            _cartItemService = cartItemService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _productService = productService;
            _httpClientFactory = httpClientFactory;
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
            // --- GA4 VIEW_CART ENTEGRASYONU BAŞLANGICI ---
            string clientId = Guid.NewGuid().ToString();
            if (Request.Cookies.TryGetValue("_ga", out string? gaCookie))
            {
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            // Sadece sepet boş değilse GA4'e veri gönderelim
            if (cartDto.CartItems != null && cartDto.CartItems.Any())
            {
                var ga4Payload = new
                {
                    client_id = clientId,
                    events = new[]
                    {
            new
            {
                name = "view_cart", // Etkinlik adı: Sepeti görüntüleme
                @params = new
                {
                    currency = "TRY",
                    value = cartDto.CartTotalAmount, // Senin 28. satırda hesapladığın toplam tutar
                    items = cartDto.CartItems.Select(item => new
                    {
                        item_id = item.ProductId.ToString(),
                        item_name = item.ProductName,
                        price = item.Price,
                        quantity = item.Quantity
                    }).ToArray() // Senin Dto'ndaki listeyi GA4'ün beklediği formata çeviriyoruz
                }
            }
        }
                };

                string measurementId = "G-X8H3TG9MKJ"; // Kendi kimliğini yaz
                string apiSecret = "ckq2ILLbQnmfSOn_9vrRQQ";       // Kendi gizli anahtarını yaz

                // Canlı URL'yi kullanıyoruz
                string ga4Url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

                try
                {
                    var client = _httpClientFactory.CreateClient();
                    await client.PostAsJsonAsync(ga4Url, ga4Payload);
                }
                catch (Exception)
                {
                    // Analitik hatası sepetin görüntülenmesini engellemesin
                }
            }
            // --- GA4 ENTEGRASYONU BİTİŞİ ---
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

            // --- GA4 ADD_TO_CART ENTEGRASYONU BAŞLANGICI ---
            string clientId = Guid.NewGuid().ToString();
            if (Request.Cookies.TryGetValue("_ga", out string? gaCookie))
            {
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            var ga4Payload = new
            {
                client_id = clientId,
                events = new[]
                {
                     new
                     {
                        name = "add_to_cart", // Sepete ekleme etkinliği
                        @params = new
                        {
                            currency = "TRY",
                            value = product.Price, // Eklenen ürünün fiyatı
                            items = new[]
                            {
                                new
                                {
                                    item_id = product.Id.ToString(),
                                    item_name = product.Name,
                                    price = product.Price,
                                    quantity = 1 // Bu metot her çalıştığında ürün 1 adet ekleniyor/artırılıyor
                                }
                            }
                        }
                     }
                }
            };

            string measurementId = "G-X8H3TG9MKJ"; // Senin kimliğin
            string apiSecret = "ckq2ILLbQnmfSOn_9vrRQQ";       // Senin gizli anahtarın

            // Canlıya alırken "debug/" kısmını silmeyi unutma
            string ga4Url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(ga4Url, ga4Payload);
            }
            catch (Exception)
            {
                // Hata durumunda müşterinin sepet işlemi kesintiye uğramasın
            }
            // --- GA4 ENTEGRASYONU BİTİŞİ ---

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

            // --- GA4 MEASUREMENT PROTOCOL ENTEGRASYONU BAŞLANGICI ---

            string clientId = Guid.NewGuid().ToString();
            if (Request.Cookies.TryGetValue("_ga", out string? gaCookie))
            {
                var cookieParts = gaCookie.Split('.');
                if (cookieParts.Length >= 4)
                {
                    clientId = $"{cookieParts[2]}.{cookieParts[3]}";
                }
            }

            var ga4Payload = new
            {
                client_id = clientId,
                events = new[]
                {
        new
        {
            name = "purchase",
            @params = new
            {
                // Görseldeki senin "newOrder" değişkeninden ID ve Toplam Fiyatı alıyoruz
                transaction_id = newOrder.Id.ToString(),
                value = newOrder.TotalPrice,
                currency = "TRY"
            }
        }
    }
            };

            string measurementId = "G-X8H3TG9MKJ"; // Panelden aldığın kimlik
            string apiSecret = "ckq2ILLbQnmfSOn_9vrRQQ";       // Panelden oluşturduğun gizli anahtar

            // Test için debug endpoint'i
            string ga4Url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                // CartController'ın constructor'ına IHttpClientFactory eklemeyi unutma!
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(ga4Url, ga4Payload);
            }
            catch (Exception)
            {
                // Analitik hatası müşterinin siparişini bozmasın diye sessizce yakalıyoruz
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