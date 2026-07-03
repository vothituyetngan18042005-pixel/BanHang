using Microsoft.AspNetCore.Mvc;
using BanHang.Data;

namespace BanHang.Controllers
{
    public class LaptopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaptopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();

            return View(products);
        }
    }
}