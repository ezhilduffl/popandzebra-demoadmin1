using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;

namespace PopZebra.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            ViewBag.TotalHome = await _db.HomeSections.CountAsync();
            ViewBag.TotalAbout = await _db.AboutSections
                                     .CountAsync(a => a.IsActive); // ← active only
            ViewBag.TotalWork = await _db.WorkItems.CountAsync();
            ViewBag.TotalShop = await _db.ShopItems.CountAsync();

            return View();
        }
    }
}