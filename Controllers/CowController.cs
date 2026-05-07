using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class CowsController : Controller
{
    private readonly ApplicationDbContext _context;

    public CowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Manage(int? id)
    {
        var cows = await _context.Cows.AsNoTracking()
            .OrderBy(c => c.CowId)
            .Select(c => new Cow
            {
                CowId = c.CowId,
                CowName = c.CowName,
                EarTag = c.EarTag ?? string.Empty,
                Color = c.Color ?? string.Empty,
                Breed = c.Breed ?? string.Empty,
                Age = c.Age,
                DateOfBirth = EF.Property<DateTime?>(c, nameof(Cow.DateOfBirth)),
                InactiveDate = EF.Property<DateTime?>(c, nameof(Cow.InactiveDate)),
                WeightAtBirth = c.WeightAtBirth,
                Pasture = c.Pasture ?? string.Empty,
                IsActive = c.IsActive,
                InactiveReason = c.InactiveReason
            })
            .ToListAsync();
        // load statuses for dropdown
        var statuses = await _context.CowStatuses.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        Cow draft;
        if (id.HasValue)
        {
            draft = await _context.Cows.AsNoTracking().FirstOrDefaultAsync(c => c.CowId == id.Value) ?? new Cow();
            // Ensure Draft includes inactive reason and status
            draft.InactiveReason = draft.InactiveReason;
        }
        else
        {
            draft = new Cow();
        }

        return View("~/Views/Cow/Manage.cshtml", new CowManageViewModel { Cows = cows, Draft = draft, Statuses = statuses });
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(int id)
    {
        try
        {
            var draft = await _context.Cows.AsNoTracking().FirstOrDefaultAsync(c => c.CowId == id) ?? new Cow();
            // return partial using explicit path to avoid view location issues
            return PartialView("~/Views/Cows/_EditModal.cshtml", draft);
        }
        catch (Exception ex)
        {
            var msg = $"ERROR rendering EditModal: {ex.Message}\n{ex.StackTrace}";
            return Content(msg, "text/plain");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCow(
        [Bind("CowId,CowName,EarTag,Color,Breed,Age,DateOfBirth,InactiveDate,WeightAtBirth,Pasture,IsActive,InactiveReason", Prefix = "Draft")] Cow draft)
    {
        // Server-side validation
        if (string.IsNullOrWhiteSpace(draft.CowName))
        {
            ModelState.AddModelError(nameof(draft.CowName), "Cow name is required.");
        }
        if (string.IsNullOrWhiteSpace(draft.Breed))
        {
            ModelState.AddModelError(nameof(draft.Breed), "Breed is required.");
        }

        if (!ModelState.IsValid)
        {
            // If this was an AJAX modal post, return partial with validation to display inside modal
            if (Request.Headers.XRequestedWith == "XMLHttpRequest" || Request.Form["IsAjax"] == "true")
            {
                Response.StatusCode = 400;
                return PartialView("~/Views/Cows/_EditModal.cshtml", draft);
            }

            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowId).ToListAsync();
            return View("~/Views/Cow/Manage.cshtml", new CowManageViewModel { Cows = cows, Draft = draft });
        }

        // If setting inactive, require a reason
        if (!draft.IsActive && string.IsNullOrWhiteSpace(draft.InactiveReason))
        {
            ModelState.AddModelError("Draft.InactiveReason", "Inactive reason is required when marking a cow inactive.");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true")
            {
                Response.StatusCode = 400;
                return PartialView("~/Views/Cows/_EditModal.cshtml", draft);
            }
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowId).ToListAsync();
            return View("~/Views/Cow/Manage.cshtml", new CowManageViewModel { Cows = cows, Draft = draft });
        }

        // If setting inactive, require InactiveDate
        if (!draft.IsActive && !draft.InactiveDate.HasValue)
        {
            ModelState.AddModelError("Draft.InactiveDate", "Inactive date is required when marking a cow inactive.");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true")
            {
                Response.StatusCode = 400;
                return PartialView("~/Views/Cows/_EditModal.cshtml", draft);
            }
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowId).ToListAsync();
            return View("~/Views/Cow/Manage.cshtml", new CowManageViewModel { Cows = cows, Draft = draft });
        }

        // Keep data clean: no reason should be stored for active cows.
        if (draft.IsActive)
        {
            draft.InactiveReason = null;
            draft.InactiveDate = null;
        }

        // Uniqueness check: CowName and EarTag must be unique
        var duplicate = await _context.Cows.AsNoTracking()
            .AnyAsync(c => (c.CowName == draft.CowName || c.EarTag == draft.EarTag) && c.CowId != draft.CowId);
        if (duplicate)
        {
            var msg = "Cow with same name or ear tag already exists.";
            // If AJAX request, return JSON so client can show popup
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { duplicate = true, message = msg });
            }

            ModelState.AddModelError(string.Empty, msg);
            TempData["Error"] = msg;
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowId).ToListAsync();
            return View("~/Views/Cow/Manage.cshtml", new CowManageViewModel { Cows = cows, Draft = draft });
        }

        if (draft.CowId == 0)
        {
            _context.Cows.Add(draft);
        }
        else
        {
            _context.Cows.Update(draft); // Update existing cow
        }

        await _context.SaveChangesAsync();
        // If AJAX/modal post, return JSON so client can close modal and refresh list without full reload
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form["IsAjax"] == "true")
        {
            return Json(new { success = true, redirect = Url.Action(nameof(Manage)) });
        }

        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCow(int id)
    {
        try
        {
            var entity = await _context.Cows.FindAsync(id);
            if (entity != null)
            {
                // Soft-delete: mark inactive instead of removing to preserve ids
                entity.IsActive = false;
                _context.Cows.Update(entity);
                await _context.SaveChangesAsync();
            } 
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Is cow se linked records (milk, health, breeding) pehle delete karen.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { error = TempData["Error"] ?? "Delete failed." });
            }
        }

        // If AJAX request, return JSON instructing client to navigate to Manage to avoid leaving history on Delete URL
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, redirect = Url.Action(nameof(Manage)) });
        }

        return RedirectToAction(nameof(Manage));
    }
}
