using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class MilkProductionController : Controller
{
    private readonly ApplicationDbContext _context;

    public MilkProductionController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id, int? filterCowId, DateTime? filterDate, string? search)
    {
        var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CowName).ToListAsync();
        var query = _context.MilkProductions.AsNoTracking().AsQueryable();

        // Only show productions for cows that currently exist in the cow list
        var cowIds = cows.Select(c => c.CowId).ToList();
        query = query.Where(m => cowIds.Contains(m.CowId));

        if (filterCowId.HasValue && filterCowId.Value > 0)
        {
            query = query.Where(m => m.CowId == filterCowId.Value);
        }
        if (filterDate.HasValue)
        {
            var d = filterDate.Value.Date;
            query = query.Where(m => m.DateProd.Date == d);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.CowName.Contains(term));
        }

        var list = await query.OrderByDescending(m => m.DateProd).ThenBy(m => m.MId).ToListAsync();

        MilkProduction draft;
        if (id.HasValue)
        {
            draft = await _context.MilkProductions.AsNoTracking().FirstOrDefaultAsync(m => m.MId == id.Value)
                ?? new MilkProduction { DateProd = DateTime.Today };
        }
        else
        {
            draft = new MilkProduction
            {
                DateProd = DateTime.Today,
                CowId = cows.FirstOrDefault()?.CowId ?? 0
            };
        }

        return View(new MilkProductionPageViewModel
        {
            Productions = list,
            Cows = cows,
            Draft = draft,
            FilterCowId = filterCowId,
            FilterDate = filterDate,
            Search = search ?? string.Empty
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(int mid)
    {
        var draft = await _context.MilkProductions.AsNoTracking().FirstOrDefaultAsync(m => m.MId == mid) ?? new MilkProduction { DateProd = DateTime.Today };
        // include active cows, but also include the draft's cow even if inactive so edit shows current value
        var cows = await _context.Cows.AsNoTracking()
            .Where(c => c.IsActive || c.CowId == draft.CowId)
            .OrderBy(c => c.CowName)
            .ToListAsync();
        return PartialView("~/Views/MilkProduction/_EditModal.cshtml", new MilkProductionPageViewModel { Draft = draft, Cows = cows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind("MId,CowId,DateProd,AmMilk,NoonMilk,PmMilk,Status", Prefix = "Draft")] MilkProduction draft,
        [FromForm(Name = "SelectedCowName")] string? selectedCowName,
        [FromForm(Name = "SelectedCowEarTag")] string? selectedCowEarTag)
    {
        // Required + range validation for server-side safety.
        if (draft.CowId <= 0)
        {
            ModelState.AddModelError(nameof(draft.CowId), "Cow selection is required.");
        }
        if (draft.DateProd == default)
        {
            ModelState.AddModelError(nameof(draft.DateProd), "Production date is required.");
        }
        if (draft.AmMilk < 0 || draft.NoonMilk < 0 || draft.PmMilk < 0)
        {
            ModelState.AddModelError(string.Empty, "Milk values cannot be negative.");
        }

        var normalizedName = (selectedCowName ?? string.Empty).Trim();
        var normalizedTag = (selectedCowEarTag ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedName) || string.IsNullOrWhiteSpace(normalizedTag))
        {
            ModelState.AddModelError(nameof(draft.CowId), "Please select a valid cow from the list.");
        }

        var cow = await _context.Cows.AsNoTracking().FirstOrDefaultAsync(c =>
            c.CowId == draft.CowId &&
            c.IsActive &&
            c.CowName == normalizedName &&
            (c.EarTag ?? string.Empty) == normalizedTag);

        if (cow == null)
        {
            ModelState.AddModelError(nameof(draft.CowId), "Selected cow does not exist in active cow list (name/ear tag mismatch).");
        }

        // Approved records are immutable.
        if (draft.MId > 0)
        {
            var existing = await _context.MilkProductions.AsNoTracking().FirstOrDefaultAsync(m => m.MId == draft.MId);
            if (existing != null && string.Equals(existing.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Approved record cannot be edited.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Duplicate prevention: one cow + one date only once.
        var hasDuplicate = await _context.MilkProductions.AsNoTracking()
            .AnyAsync(m => m.CowId == draft.CowId && m.DateProd.Date == draft.DateProd.Date && m.MId != draft.MId);
        if (hasDuplicate)
        {
            // If this is an AJAX/modal submit, return a clear JSON payload so client can show a message without replacing the modal markup.
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true";
            if (isAjax)
            {
                return Json(new { duplicate = true, message = "Duplicate entry: this cow already has a record for this date." });
            }

            ModelState.AddModelError(string.Empty, "Duplicate entry: this cow already has a record for this date.");
        }

        if (string.IsNullOrWhiteSpace(draft.Status))
        {
            draft.Status = "Draft";
        }
        if (draft.Status != "Draft" && draft.Status != "Approved")
        {
            draft.Status = "Draft";
        }

        if (!ModelState.IsValid)
        {
            var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CowName).ToListAsync();
            // ensure the list only contains productions for existing cows
            var validCowIds = cows.Select(c => c.CowId).ToList();
            var list = await _context.MilkProductions.AsNoTracking()
                .Where(m => validCowIds.Contains(m.CowId))
                .OrderByDescending(m => m.DateProd).ToListAsync();
            Response.StatusCode = 400;
            if (Request.Form["IsAjax"] == "true")
            {
                return PartialView("~/Views/MilkProduction/_EditModal.cshtml",
                    new MilkProductionPageViewModel { Draft = draft, Cows = cows });
            }
            return View("Index", new MilkProductionPageViewModel { Productions = list, Cows = cows, Draft = draft });
        }

        // Final existence guard: ensure cow still exists before persisting (prevents tampered/invalid ids)
        // check existence among active cows
        var realCow = await _context.Cows.AsNoTracking().FirstOrDefaultAsync(c => c.CowId == draft.CowId && c.IsActive);
        if (realCow == null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true";
            if (isAjax)
            {
                return Json(new { success = false, message = "Please select a valid cow." });
            }

            ModelState.AddModelError(nameof(draft.CowId), "Please select a valid cow.");
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowName).ToListAsync();
            var validCowIds = cows.Select(c => c.CowId).ToList();
            var list = await _context.MilkProductions.AsNoTracking()
                .Where(m => validCowIds.Contains(m.CowId))
                .OrderByDescending(m => m.DateProd).ToListAsync();
            Response.StatusCode = 400;
            if (Request.Form["IsAjax"] == "true")
            {
                return PartialView("~/Views/MilkProduction/_EditModal.cshtml",
                    new MilkProductionPageViewModel { Draft = draft, Cows = cows });
            }
            return View("Index", new MilkProductionPageViewModel { Productions = list, Cows = cows, Draft = draft });
        }

        // If a CowName was supplied in the form, require it to match the real cow's name.
        if (!string.IsNullOrWhiteSpace(draft.CowName) &&
            !string.Equals(draft.CowName?.Trim(), realCow.CowName?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true";
            if (isAjax)
            {
                return Json(new { success = false, message = "Cow name does not match selected cow." });
            }

            ModelState.AddModelError(nameof(draft.CowName), "Cow name does not match selected cow.");
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowName).ToListAsync();
            var validCowIds = cows.Select(c => c.CowId).ToList();
            var list = await _context.MilkProductions.AsNoTracking()
                .Where(m => validCowIds.Contains(m.CowId))
                .OrderByDescending(m => m.DateProd).ToListAsync();
            Response.StatusCode = 400;
            if (Request.Form["IsAjax"] == "true")
            {
                return PartialView("~/Views/MilkProduction/_EditModal.cshtml",
                    new MilkProductionPageViewModel { Draft = draft, Cows = cows });
            }
            return View("Index", new MilkProductionPageViewModel { Productions = list, Cows = cows, Draft = draft });
        }

        // canonicalize stored name from DB
        draft.CowName = realCow.CowName;
        if (draft.MId == 0)
        {
            _context.MilkProductions.Add(draft);
            TempData["Success"] = "Milk production record created successfully.";
        }
        else
        {
            _context.MilkProductions.Update(draft);
            TempData["Success"] = "Milk production record updated successfully.";
        }

        await _context.SaveChangesAsync();
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true")
        {
            return Json(new { success = true, redirect = Url.Action(nameof(Index)) });
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var isAjax = Request.Headers.XRequestedWith == "XMLHttpRequest" || Request.Form["IsAjax"] == "true";
        var entity = await _context.MilkProductions.FindAsync(id);
        if (entity == null)
        {
            if (isAjax)
            {
                return Json(new { success = false, message = "Record not found.", redirect = Url.Action(nameof(Index)) });
            }
            TempData["Error"] = "Record not found.";
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(entity.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (isAjax)
            {
                return Json(new { success = false, message = "Approved record cannot be deleted.", redirect = Url.Action(nameof(Index)) });
            }
            TempData["Error"] = "Approved record cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.MilkProductions.Remove(entity);
        await _context.SaveChangesAsync();
        if (isAjax)
        {
            return Json(new { success = true, redirect = Url.Action(nameof(Index)) });
        }
        TempData["Success"] = "Milk production record deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
