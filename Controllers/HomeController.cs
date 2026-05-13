using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BanHang.Models;
using BanHang.Data;

namespace BanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Constructor
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ================= HOME =================

        public IActionResult Index()
        {
            // Lấy danh sách sản phẩm
            var products = _context.Products.ToList();

            return View(products);
        }

        // ================= PRIVACY =================

        public IActionResult Privacy()
        {
            return View();
        }

        // ================= ERROR =================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new Error
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}