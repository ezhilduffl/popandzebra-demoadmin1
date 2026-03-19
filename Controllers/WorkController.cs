using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopZebra.Data;
using PopZebra.Models;
using PopZebra.Services;

namespace PopZebra.Controllers
{
    [Authorize]
    public class WorkController : Controller
    {
        private readonly AppDbContext _db;
        private readonly FileUploadService _upload;
        private const int PageSize = 10;
        private const int MaxSlots = 10; // ← changed from 5 to 10

        public WorkController(AppDbContext db, FileUploadService upload)
        {
            _db = db;
            _upload = upload;
        }

        private async Task<List<int>> GetAvailableOrdersAsync(int excludeId = 0)
        {
            var taken = await _db.WorkItems
                .Where(w => w.Id != excludeId)
                .Select(w => w.DisplayOrder)
                .ToListAsync();

            return Enumerable.Range(1, MaxSlots)
                .Where(n => !taken.Contains(n))
                .ToList();
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            ViewData["Title"] = "Work Module";
            if (page < 1) page = 1;

            var query = _db.WorkItems.OrderBy(w => w.DisplayOrder);
            var paged = await PaginatedList<WorkItem>
                .CreateAsync(query, page, PageSize);

            ViewBag.CanAdd = await _db.WorkItems.CountAsync() < MaxSlots;
            return View(paged);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Work - Details";
            var item = await _db.WorkItems.FindAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Work - Create";
            var available = await GetAvailableOrdersAsync();
            if (!available.Any())
            {
                TempData["Error"] =
                    "All 10 display order slots are taken. " +
                    "Delete an existing item to add a new one.";
                return RedirectToAction(nameof(Index));
            }
            return View(new WorkItemViewModel { AvailableOrders = available });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkItemViewModel vm)
        {
            ViewData["Title"] = "Work - Create";

            if (vm.ImageFile == null || vm.ImageFile.Length == 0)
                ModelState.AddModelError("ImageFile",
                    "Please upload an image.");

            if (vm.DisplayOrder.HasValue)
            {
                bool taken = await _db.WorkItems
                    .AnyAsync(w => w.DisplayOrder == vm.DisplayOrder.Value);
                if (taken)
                    ModelState.AddModelError("DisplayOrder",
                        $"Display order {vm.DisplayOrder} is already taken.");
            }

            if (!ModelState.IsValid)
            {
                vm.AvailableOrders = await GetAvailableOrdersAsync();
                return View(vm);
            }

            var r = await _upload.SaveFileAsync(vm.ImageFile!, "work");
            if (!r.Success)
            {
                ModelState.AddModelError("ImageFile", r.Error!);
                vm.AvailableOrders = await GetAvailableOrdersAsync();
                return View(vm);
            }

            _db.WorkItems.Add(new WorkItem
            {
                Title = vm.Title.Trim(),
                LinkUrl = vm.LinkUrl.Trim(),
                ImagePath = r.FilePath!,
                DisplayOrder = vm.DisplayOrder!.Value,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Work item created successfully.";
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> Edit(int id)
        //{
        //    ViewData["Title"] = "Work - Edit";
        //    var item = await _db.WorkItems.FindAsync(id);
        //    if (item is null) return NotFound();

        //    var available = await GetAvailableOrdersAsync(excludeId: id);

        //    // Only add current order if not already in the list
        //    if (!available.Contains(item.DisplayOrder))
        //        available.Add(item.DisplayOrder);

        //    available.Sort();

        //    return View(new WorkItemViewModel
        //    {
        //        Id = item.Id,
        //        Title = item.Title,
        //        LinkUrl = item.LinkUrl,
        //        DisplayOrder = item.DisplayOrder,
        //        ExistingImagePath = item.ImagePath,
        //        AvailableOrders = available,
        //        CreatedOn = item.CreatedOn,
        //        UpdatedOn = item.UpdatedOn
        //    });
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, WorkItemViewModel vm)
        //{
        //    ViewData["Title"] = "Work - Edit";

        //    if (vm.DisplayOrder.HasValue)
        //    {
        //        bool taken = await _db.WorkItems
        //            .AnyAsync(w => w.DisplayOrder == vm.DisplayOrder.Value
        //                       && w.Id != id);
        //        if (taken)
        //            ModelState.AddModelError("DisplayOrder",
        //                $"Display order {vm.DisplayOrder} is already taken.");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        var avail = await GetAvailableOrdersAsync(excludeId: id);
        //        if (vm.DisplayOrder.HasValue &&
        //            !avail.Contains(vm.DisplayOrder.Value))
        //            avail.Add(vm.DisplayOrder.Value);
        //        avail.Sort();
        //        vm.AvailableOrders = avail;
        //        return View(vm);
        //    }

        //    var entity = await _db.WorkItems.FindAsync(id);
        //    if (entity is null) return NotFound();

        //    if (vm.ImageFile != null && vm.ImageFile.Length > 0)
        //    {
        //        var r = await _upload.SaveFileAsync(vm.ImageFile, "work");
        //        if (!r.Success)
        //        {
        //            ModelState.AddModelError("ImageFile", r.Error!);
        //            var avail = await GetAvailableOrdersAsync(excludeId: id);
        //            if (!avail.Contains(vm.DisplayOrder!.Value))
        //                avail.Add(vm.DisplayOrder.Value);
        //            avail.Sort();
        //            vm.AvailableOrders = avail;
        //            return View(vm);
        //        }
        //        _upload.DeleteFile(entity.ImagePath);
        //        entity.ImagePath = r.FilePath!;
        //    }

        //    entity.Title = vm.Title.Trim();
        //    entity.LinkUrl = vm.LinkUrl.Trim();
        //    entity.DisplayOrder = vm.DisplayOrder!.Value;
        //    entity.UpdatedOn = DateTime.UtcNow;

        //    await _db.SaveChangesAsync();

        //    TempData["Success"] = "Work item updated successfully.";
        //    return RedirectToAction(nameof(Index));
        //}


        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Work - Edit";
            var item = await _db.WorkItems.FindAsync(id);
            if (item is null) return NotFound();

            // ── Always show ALL 10 orders ─────────────────────────
            var allOrders = Enumerable.Range(1, MaxSlots).ToList();

            // ── Get taken orders with their item ids ──────────────
            var takenOrders = await _db.WorkItems
                .Where(w => w.Id != id)
                .Select(w => new { w.DisplayOrder, w.Id, w.Title })
                .ToListAsync();

            // Pass taken order info to view as JSON
            var takenOrdersJson = System.Text.Json.JsonSerializer
                .Serialize(takenOrders.Select(t => new
                {
                    order = t.DisplayOrder,
                    id = t.Id,
                    title = t.Title
                }));

            ViewBag.TakenOrdersJson = takenOrdersJson;

            return View(new WorkItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                LinkUrl = item.LinkUrl,
                DisplayOrder = item.DisplayOrder,
                ExistingImagePath = item.ImagePath,
                AvailableOrders = allOrders,   // ALL 10 always
                CreatedOn = item.CreatedOn,
                UpdatedOn = item.UpdatedOn
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkItemViewModel vm,
     bool replaceOrder = false)
        {
            ViewData["Title"] = "Work - Edit";

            if (!vm.DisplayOrder.HasValue)
                ModelState.AddModelError("DisplayOrder",
                    "Please select a display order.");

            if (!ModelState.IsValid)
            {
                vm.AvailableOrders = Enumerable.Range(1, MaxSlots).ToList();
                var takenErr = await _db.WorkItems
                    .Where(w => w.Id != id)
                    .Select(w => new { w.DisplayOrder, w.Id, w.Title })
                    .ToListAsync();
                ViewBag.TakenOrdersJson = System.Text.Json.JsonSerializer
                    .Serialize(takenErr.Select(t => new
                    {
                        order = t.DisplayOrder,
                        id = t.Id,
                        title = t.Title
                    }));
                return View(vm);
            }

            var entity = await _db.WorkItems.FindAsync(id);
            if (entity is null) return NotFound();

            // ── Handle image upload first ─────────────────────────
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var r = await _upload.SaveFileAsync(vm.ImageFile, "work");
                if (!r.Success)
                {
                    ModelState.AddModelError("ImageFile", r.Error!);
                    vm.AvailableOrders = Enumerable.Range(1, MaxSlots).ToList();
                    var takenErr2 = await _db.WorkItems
                        .Where(w => w.Id != id)
                        .Select(w => new { w.DisplayOrder, w.Id, w.Title })
                        .ToListAsync();
                    ViewBag.TakenOrdersJson = System.Text.Json.JsonSerializer
                        .Serialize(takenErr2.Select(t => new
                        {
                            order = t.DisplayOrder,
                            id = t.Id,
                            title = t.Title
                        }));
                    return View(vm);
                }
                _upload.DeleteFile(entity.ImagePath);
                entity.ImagePath = r.FilePath!;
            }

            // ── Check if selected order is taken by another item ──
            var conflictItem = await _db.WorkItems
                .FirstOrDefaultAsync(w => w.DisplayOrder == vm.DisplayOrder.Value
                                       && w.Id != id);

            if (conflictItem != null && replaceOrder)
            {
                // ── Swap using temp order to avoid unique constraint ──
                // Step 1: Move current entity to a temp order (999)
                //         that is guaranteed to not exist
                int oldOrder = entity.DisplayOrder;
                int newOrder = vm.DisplayOrder!.Value;
                int tempOrder = 999;

                entity.DisplayOrder = tempOrder;
                entity.UpdatedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();  // Save temp order first

                // Step 2: Move conflict item to old order
                conflictItem.DisplayOrder = oldOrder;
                conflictItem.UpdatedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();  // Save conflict item

                // Step 3: Move current entity to new order
                entity.DisplayOrder = newOrder;
                entity.UpdatedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();  // Save final order
            }
            else if (conflictItem != null && !replaceOrder)
            {
                ModelState.AddModelError("DisplayOrder",
                    $"Order {vm.DisplayOrder} is already taken by " +
                    $"\"{conflictItem.Title}\".");

                vm.AvailableOrders = Enumerable.Range(1, MaxSlots).ToList();
                var takenErr3 = await _db.WorkItems
                    .Where(w => w.Id != id)
                    .Select(w => new { w.DisplayOrder, w.Id, w.Title })
                    .ToListAsync();
                ViewBag.TakenOrdersJson = System.Text.Json.JsonSerializer
                    .Serialize(takenErr3.Select(t => new
                    {
                        order = t.DisplayOrder,
                        id = t.Id,
                        title = t.Title
                    }));
                return View(vm);
            }
            else
            {
                // ── No conflict — just update order directly ──────
                entity.DisplayOrder = vm.DisplayOrder!.Value;
                entity.UpdatedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // ── Update other fields ───────────────────────────────
            entity.Title = vm.Title.Trim();
            entity.LinkUrl = vm.LinkUrl.Trim();
            entity.UpdatedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Work item updated successfully.";
            return RedirectToAction(nameof(Index));
        }






        public async Task<IActionResult> Delete(int id)
        {
            ViewData["Title"] = "Work - Delete";
            var item = await _db.WorkItems.FindAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.WorkItems.FindAsync(id);
            if (item is null) return NotFound();

            _upload.DeleteFile(item.ImagePath);
            _db.WorkItems.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Work item deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}