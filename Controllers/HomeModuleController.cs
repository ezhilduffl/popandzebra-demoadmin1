using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;
using PopZebra.Models;
using PopZebra.Services;

namespace PopZebra.Controllers
{
    [Authorize]
    public class HomeModuleController : Controller
    {
        private readonly AppDbContext _db;
        private readonly FileUploadService _upload;
        private const int PageSize = 10;

        public HomeModuleController(AppDbContext db, FileUploadService upload)
        {
            _db = db;
            _upload = upload;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            ViewData["Title"] = "Home Module";
            if (page < 1) page = 1;

            var query = _db.HomeSections.OrderByDescending(x => x.CreatedOn);
            var paged = await PaginatedList<HomeSection>
                .CreateAsync(query, page, PageSize);

            return View(paged);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Home - Details";
            var item = await _db.HomeSections.FindAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Home - Create";
            return View(new HomeSectionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomeSectionViewModel vm)
        {
            ViewData["Title"] = "Home - Create";

            if (vm.SetImageFor == null)
                ModelState.AddModelError("SetImageFor",
                    "Please select Yes or No for Set Image For.");
            else if (vm.SetImageFor == true)
            {
                if (vm.SingleImage == null || vm.SingleImage.Length == 0)
                    ModelState.AddModelError("SingleImage",
                        "Please upload an image.");
            }
            else
            {
                if (vm.MobileImage == null || vm.MobileImage.Length == 0)
                    ModelState.AddModelError("MobileImage",
                        "Please upload a mobile image.");
                if (vm.DesktopImage == null || vm.DesktopImage.Length == 0)
                    ModelState.AddModelError("DesktopImage",
                        "Please upload a desktop image.");
            }

            if (!ModelState.IsValid) return View(vm);

            string? singlePath = null, mobilePath = null, desktopPath = null;

            if (vm.SetImageFor == true)
            {
                var r = await _upload.SaveFileAsync(vm.SingleImage!, "home");
                if (!r.Success)
                {
                    ModelState.AddModelError("SingleImage", r.Error!);
                    return View(vm);
                }
                singlePath = r.FilePath;
            }
            else
            {
                var mr = await _upload.SaveFileAsync(vm.MobileImage!, "home");
                if (!mr.Success)
                {
                    ModelState.AddModelError("MobileImage", mr.Error!);
                    return View(vm);
                }
                mobilePath = mr.FilePath;

                var dr = await _upload.SaveFileAsync(vm.DesktopImage!, "home");
                if (!dr.Success)
                {
                    ModelState.AddModelError("DesktopImage", dr.Error!);
                    return View(vm);
                }
                desktopPath = dr.FilePath;
            }

            _db.HomeSections.Add(new HomeSection
            {
                Title = vm.Title.Trim(),
                SetImageFor = vm.SetImageFor!.Value,
                SingleImagePath = singlePath,
                MobileImagePath = mobilePath,
                DesktopImagePath = desktopPath,
                // ── NEW ─────────────────────────────────────
                LinkUrl = string.IsNullOrWhiteSpace(vm.LinkUrl)
                                   ? null : vm.LinkUrl.Trim(),
                // ────────────────────────────────────────────
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Home section created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Home - Edit";
            var item = await _db.HomeSections.FindAsync(id);
            if (item is null) return NotFound();

            return View(new HomeSectionViewModel
            {
                Id = item.Id,
                Title = item.Title,
                SetImageFor = item.SetImageFor,
                // ── NEW ─────────────────────────────────────
                LinkUrl = item.LinkUrl,
                // ────────────────────────────────────────────
                ExistingSingleImagePath = item.SingleImagePath,
                ExistingMobileImagePath = item.MobileImagePath,
                ExistingDesktopImagePath = item.DesktopImagePath,
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HomeSectionViewModel vm)
        {
            ViewData["Title"] = "Home - Edit";

            if (vm.SetImageFor == null)
                ModelState.AddModelError("SetImageFor",
                    "Please select Yes or No for Set Image For.");

            if (!ModelState.IsValid) return View(vm);

            var entity = await _db.HomeSections.FindAsync(id);
            if (entity is null) return NotFound();

            if (vm.SetImageFor == true)
            {
                if (vm.SingleImage != null && vm.SingleImage.Length > 0)
                {
                    var r = await _upload.SaveFileAsync(vm.SingleImage, "home");
                    if (!r.Success)
                    {
                        ModelState.AddModelError("SingleImage", r.Error!);
                        return View(vm);
                    }
                    _upload.DeleteFile(entity.SingleImagePath);
                    _upload.DeleteFile(entity.MobileImagePath);
                    _upload.DeleteFile(entity.DesktopImagePath);
                    entity.SingleImagePath = r.FilePath;
                    entity.MobileImagePath = null;
                    entity.DesktopImagePath = null;
                }
            }
            else
            {
                bool mu = false, du = false;
                if (vm.MobileImage != null && vm.MobileImage.Length > 0)
                {
                    var r = await _upload.SaveFileAsync(vm.MobileImage, "home");
                    if (!r.Success)
                    {
                        ModelState.AddModelError("MobileImage", r.Error!);
                        return View(vm);
                    }
                    _upload.DeleteFile(entity.MobileImagePath);
                    entity.MobileImagePath = r.FilePath;
                    mu = true;
                }
                if (vm.DesktopImage != null && vm.DesktopImage.Length > 0)
                {
                    var r = await _upload.SaveFileAsync(vm.DesktopImage, "home");
                    if (!r.Success)
                    {
                        ModelState.AddModelError("DesktopImage", r.Error!);
                        return View(vm);
                    }
                    _upload.DeleteFile(entity.DesktopImagePath);
                    entity.DesktopImagePath = r.FilePath;
                    du = true;
                }
                if (entity.SetImageFor == true)
                {
                    if (!mu)
                    {
                        ModelState.AddModelError("MobileImage",
                            "Please upload a mobile image.");
                        return View(vm);
                    }
                    if (!du)
                    {
                        ModelState.AddModelError("DesktopImage",
                            "Please upload a desktop image.");
                        return View(vm);
                    }
                    _upload.DeleteFile(entity.SingleImagePath);
                    entity.SingleImagePath = null;
                }
            }

            entity.Title = vm.Title.Trim();
            entity.SetImageFor = vm.SetImageFor!.Value;
            // ── NEW ─────────────────────────────────────────
            entity.LinkUrl = string.IsNullOrWhiteSpace(vm.LinkUrl)
                               ? null : vm.LinkUrl.Trim();
            // ────────────────────────────────────────────────
            entity.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Home section updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            ViewData["Title"] = "Home - Delete";
            var item = await _db.HomeSections.FindAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.HomeSections.FindAsync(id);
            if (item is null) return NotFound();

            _upload.DeleteFile(item.SingleImagePath);
            _upload.DeleteFile(item.MobileImagePath);
            _upload.DeleteFile(item.DesktopImagePath);

            _db.HomeSections.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Home section deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}