using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Filters;

namespace WebAdmin.Controllers
{
    [AdminAuth]
    public class OrderController : Controller
    {

        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        // Siparişleri Listeleme Sayfası
        public IActionResult Index()
        {
            var orders = _orderService.GetAll();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _orderService.GetAll().FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                
                _orderService.Update(order);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            // Ana siparişi bul
            var order = _orderService.GetAll().FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            // Bu siparişe ait ürün listesini bul
            var orderDetails = _orderDetailService.GetAll().Where(od => od.OrderId == id).ToList();

            ViewBag.OrderDetails = orderDetails; // Görünüme taşı
            return View(order);
        }
    }
}