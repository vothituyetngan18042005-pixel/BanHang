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
        // ================= LINH KIỆN =================

        public IActionResult LinhKien(string[] type)
        {
            var query = _context.Products
                .Where(x => x.Category == "LinhKien");

            if (type != null && type.Any())
            {
                query = query.Where(x => type.Contains(x.Type));
            }

            ViewBag.SelectedTypes = type ?? Array.Empty<string>();

            return View("~/Views/LinhKien/Index.cshtml", query.ToList());
        }

        // ================= LAPTOP =================

        public IActionResult Laptop(string[] brand, int[] ram)
        {
            var query = _context.Products
                .Where(x => x.Category == "Laptop");

            if (brand != null && brand.Any())
            {
                query = query.Where(x => brand.Contains(x.Brand));
            }

            if (ram != null && ram.Any())
            {
                query = query.Where(x => x.Ram.HasValue && ram.Contains(x.Ram.Value));
            }

            ViewBag.SelectedBrand = brand ?? Array.Empty<string>();
            ViewBag.SelectedRam = ram ?? Array.Empty<int>();

            return View("~/Views/Laptop/Index.cshtml", query.ToList());
        }

        // ================= PHỤ KIỆN =================

        public IActionResult PhuKien(string[] accessoryType, string[] brand)
        {
            var query = _context.Products
                .Where(x => x.Category == "PhuKien");

            if (accessoryType != null && accessoryType.Any())
            {
                query = query.Where(x => accessoryType.Contains(x.AccessoryType));
            }

            if (brand != null && brand.Any())
            {
                query = query.Where(x => brand.Contains(x.Brand));
            }

            ViewBag.SelectedAccessoryTypes = accessoryType ?? Array.Empty<string>();
            ViewBag.SelectedBrands = brand ?? Array.Empty<string>();

            return View("~/Views/PhuKien/Index.cshtml", query.ToList());
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