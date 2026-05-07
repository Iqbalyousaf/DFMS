using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class HealthController : Controller
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id)
    {
        var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CowName).ToListAsync();
        var list = await _context.HealthReports.AsNoTracking()
            .OrderByDescending(r => r.RepDate)
            .ThenBy(r => r.RepId)
            .ToListAsync();

        HealthReport draft;
        if (id.HasValue)
        {
            draft = await _context.HealthReports.AsNoTracking().FirstOrDefaultAsync(r => r.RepId == id.Value)
                ?? new HealthReport { RepDate = DateTime.Today, CowId = cows.FirstOrDefault()?.CowId ?? 0 };
        }
        else
        {
            draft = new HealthReport
            {
                RepDate = DateTime.Today,
                CowId = cows.FirstOrDefault()?.CowId ?? 0
            };
        }

        if (draft.CowId != 0)
        {
            var cn = cows.FirstOrDefault(c => c.CowId == draft.CowId)?.CowName;
            if (!string.IsNullOrEmpty(cn))
            {
                draft.CowName = cn;
            }
        }

        return View(new HealthPageViewModel { Reports = list, Cows = cows, Draft = draft });
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(int id)
    {
        var draft = await _context.HealthReports.AsNoTracking().FirstOrDefaultAsync(r => r.RepId == id) ?? new HealthReport { RepDate = DateTime.Today };
        var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive || c.CowId == draft.CowId).OrderBy(c => c.CowName).ToListAsync();
        return PartialView("~/Views/Health/_EditModal.cshtml", new HealthPageViewModel { Draft = draft, Cows = cows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind("RepId,CowId,RepDate,Event,Diagnosis,Treatment,Cost,VetName", Prefix = "Draft")] HealthReport draft)
    {
        var cow = await _context.Cows.FindAsync(draft.CowId);
        if (cow == null)
        {
            ModelState.AddModelError(nameof(draft.CowId), "Cow select karen.");
        }

        if (!ModelState.IsValid)
        {
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowName).ToListAsync();
            var list = await _context.HealthReports.AsNoTracking().OrderByDescending(r => r.RepDate).ToListAsync();
            return View("Index", new HealthPageViewModel { Reports = list, Cows = cows, Draft = draft });
        }

        draft.CowName = cow!.CowName;
        if (draft.RepId == 0)
        {
            _context.HealthReports.Add(draft);
        }
        else
        {
            _context.HealthReports.Update(draft);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.HealthReports.FindAsync(id);
        if (entity != null)
        {
            _context.HealthReports.Remove(entity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
