using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;
using PopZebra.Models;
using PopZebra.Services;

namespace PopZebra.Controllers
{
    [Authorize]
    public class AboutController : Controller
    {
        private readonly AppDbContext _db;
        private readonly FileUploadService _upload;
        private const int PageSize = 10;

        public AboutController(AppDbContext db, FileUploadService upload)
        {
            _db = db;
            _upload = upload;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            ViewData["Title"] = "About Module";
            if (page < 1) page = 1;

            var query = _db.AboutSections
                .Include(a => a.Icons.OrderBy(i => i.SortOrder))
                .OrderByDescending(a => a.CreatedOn);

            var paged = await PaginatedList<AboutSection>
                .CreateAsync(query, page, PageSize);

            return View(paged);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "About - Details";
            var item = await _db.AboutSections
                .Include(a => a.Icons.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(a => a.Id == id);
            if (item is null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "About - Create";
            return View(new AboutSectionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AboutSectionViewModel vm)
        {
            ViewData["Title"] = "About - Create";

            for (int i = 0; i < vm.Icons.Count; i++)
            {
                var icon = vm.Icons[i];
                if (string.IsNullOrWhiteSpace(icon.Title))
                    ModelState.AddModelError($"Icons[{i}].Title",
                        $"Icon {i + 1} title is required.");
                if (icon.IconFile == null || icon.IconFile.Length == 0)
                    ModelState.AddModelError($"Icons[{i}].IconFile",
                        $"Icon {i + 1} image is required.");
                if (string.IsNullOrWhiteSpace(icon.LinkUrl))
                    ModelState.AddModelError($"Icons[{i}].LinkUrl",
                        $"Icon {i + 1} link URL is required.");
            }

            if (!ModelState.IsValid) return View(vm);

            var iconEntities = new List<AboutIcon>();
            for (int i = 0; i < vm.Icons.Count; i++)
            {
                var icon = vm.Icons[i];
                var result = await _upload.SaveFileAsync(icon.IconFile!, "about");
                if (!result.Success)
                {
                    ModelState.AddModelError($"Icons[{i}].IconFile", result.Error!);
                    return View(vm);
                }
                iconEntities.Add(new AboutIcon
                {
                    SortOrder = i + 1,
                    Title = icon.Title.Trim(),
                    IconPath = result.FilePath!,
                    LinkUrl = icon.LinkUrl.Trim()
                });
            }

            _db.AboutSections.Add(new AboutSection
            {
                Content = vm.Content.Trim(),
                IsActive = vm.IsActive,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                Icons = iconEntities
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "About section created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "About - Edit";
            var item = await _db.AboutSections
                .Include(a => a.Icons.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(a => a.Id == id);
            if (item is null) return NotFound();

            var vm = new AboutSectionViewModel
            {
                Id = item.Id,
                Content = item.Content,
                IsActive = item.IsActive,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn,
                Icons = item.Icons.Select((icon, idx) => new AboutIconViewModel
                {
                    Id = icon.Id,
                    Title = icon.Title,
                    LinkUrl = icon.LinkUrl,
                    ExistingIconPath = icon.IconPath,
                    SortOrder = idx + 1
                }).ToList()
            };

            while (vm.Icons.Count < 5)
                vm.Icons.Add(new AboutIconViewModel
                { SortOrder = vm.Icons.Count + 1 });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AboutSectionViewModel vm)
        {
            ViewData["Title"] = "About - Edit";

            for (int i = 0; i < vm.Icons.Count; i++)
            {
                var icon = vm.Icons[i];
                if (string.IsNullOrWhiteSpace(icon.Title))
                    ModelState.AddModelError($"Icons[{i}].Title",
                        $"Icon {i + 1} title is required.");
                if (string.IsNullOrWhiteSpace(icon.LinkUrl))
                    ModelState.AddModelError($"Icons[{i}].LinkUrl",
                        $"Icon {i + 1} link URL is required.");
                if (icon.Id == 0 &&
                    (icon.IconFile == null || icon.IconFile.Length == 0))
                    ModelState.AddModelError($"Icons[{i}].IconFile",
                        $"Icon {i + 1} image is required.");
            }

            if (!ModelState.IsValid) return View(vm);

            var entity = await _db.AboutSections
                .Include(a => a.Icons)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null) return NotFound();

            entity.Content = vm.Content.Trim();
            entity.IsActive = vm.IsActive;
            entity.UpdatedOn = DateTime.UtcNow;

            var oldIcons = entity.Icons.ToList();

            for (int i = 0; i < vm.Icons.Count; i++)
            {
                var iconVm = vm.Icons[i];
                var existing = oldIcons.FirstOrDefault(x => x.Id == iconVm.Id);
                string iconPath;

                if (iconVm.IconFile != null && iconVm.IconFile.Length > 0)
                {
                    var result = await _upload.SaveFileAsync(iconVm.IconFile, "about");
                    if (!result.Success)
                    {
                        ModelState.AddModelError($"Icons[{i}].IconFile", result.Error!);
                        return View(vm);
                    }
                    if (existing != null) _upload.DeleteFile(existing.IconPath);
                    iconPath = result.FilePath!;
                }
                else
                {
                    iconPath = existing?.IconPath ?? string.Empty;
                }

                if (existing != null)
                {
                    existing.Title = iconVm.Title.Trim();
                    existing.IconPath = iconPath;
                    existing.LinkUrl = iconVm.LinkUrl.Trim();
                    existing.SortOrder = i + 1;
                }
                else
                {
                    entity.Icons.Add(new AboutIcon
                    {
                        SortOrder = i + 1,
                        Title = iconVm.Title.Trim(),
                        IconPath = iconPath,
                        LinkUrl = iconVm.LinkUrl.Trim()
                    });
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "About section updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            ViewData["Title"] = "About - Delete";
            var item = await _db.AboutSections
                .Include(a => a.Icons)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.AboutSections
                .Include(a => a.Icons)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (item is null) return NotFound();

            foreach (var icon in item.Icons)
                _upload.DeleteFile(icon.IconPath);

            _db.AboutSections.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "About section deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] int id)
        {
            var item = await _db.AboutSections.FindAsync(id);
            if (item is null)
                return Json(new { success = false, message = "Record not found." });

            item.IsActive = !item.IsActive;
            item.UpdatedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = item.IsActive,
                label = item.IsActive ? "Active" : "Inactive"
            });
        }
    }
}