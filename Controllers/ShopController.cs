using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;
using PopZebra.Models;
using PopZebra.Services;

namespace PopZebra.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;
        private readonly FileUploadService _upload;
        private const int PageSize = 10;

        public ShopController(AppDbContext db, FileUploadService upload)
        {
            _db = db;
            _upload = upload;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            ViewData["Title"] = "Shop Module";
            if (page < 1) page = 1;

            var query = _db.ShopItems
                .Include(s => s.History)
                .OrderByDescending(s => s.CreatedOn);

            var paged = await PaginatedList<ShopItem>
                .CreateAsync(query, page, PageSize);

            return View(paged);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Shop - Details";
            var item = await _db.ShopItems
                .Include(s => s.History.OrderByDescending(h => h.SavedOn))
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item is null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Shop - Create";
            return View(new ShopItemViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopItemViewModel vm)
        {
            ViewData["Title"] = "Shop - Create";

            if (vm.ImageFile == null || vm.ImageFile.Length == 0)
                ModelState.AddModelError("ImageFile", "Please upload an image.");

            if (!ModelState.IsValid) return View(vm);

            var r = await _upload.SaveFileAsync(vm.ImageFile!, "shop");
            if (!r.Success)
            {
                ModelState.AddModelError("ImageFile", r.Error!);
                return View(vm);
            }

            _db.ShopItems.Add(new ShopItem
            {
                Content = vm.Content.Trim(),
                ImagePath = r.FilePath!,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Shop item created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Shop - Edit";
            var item = await _db.ShopItems.FindAsync(id);
            if (item is null) return NotFound();

            return View(new ShopItemViewModel
            {
                Id = item.Id,
                Content = item.Content,
                ExistingImagePath = item.ImagePath,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShopItemViewModel vm)
        {
            ViewData["Title"] = "Shop - Edit";

            if (!ModelState.IsValid) return View(vm);

            var entity = await _db.ShopItems.FindAsync(id);
            if (entity is null) return NotFound();

            // Snapshot before update
            _db.ShopItemHistories.Add(new ShopItemHistory
            {
                ShopItemId = entity.Id,
                Content = entity.Content,
                ImagePath = entity.ImagePath,
                SavedOn = DateTime.UtcNow
            });

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var r = await _upload.SaveFileAsync(vm.ImageFile, "shop");
                if (!r.Success)
                {
                    ModelState.AddModelError("ImageFile", r.Error!);
                    return View(vm);
                }
                entity.ImagePath = r.FilePath!;
            }

            entity.Content = vm.Content.Trim();
            entity.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Shop item updated. Previous version saved to history.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            ViewData["Title"] = "Shop - Delete";
            var item = await _db.ShopItems
                .Include(s => s.History)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.ShopItems
                .Include(s => s.History)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item is null) return NotFound();

            _upload.DeleteFile(item.ImagePath);
            foreach (var h in item.History)
                _upload.DeleteFile(h.ImagePath);

            _db.ShopItems.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Shop item and all history deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}