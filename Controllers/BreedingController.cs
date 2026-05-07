using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class BreedingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BreedingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id)
    {
        var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CowName).ToListAsync();
        var list = await _context.BreedReports.AsNoTracking()
            .OrderByDescending(r => r.BreedDate)
            .ThenBy(r => r.Brid)
            .ToListAsync();

        BreedReport draft;
        if (id.HasValue)
        {
            draft = await _context.BreedReports.AsNoTracking().FirstOrDefaultAsync(r => r.Brid == id.Value)
                ?? new BreedReport { CowId = cows.FirstOrDefault()?.CowId ?? 0 };
        }
        else
        {
            draft = new BreedReport
            {
                CowId = cows.FirstOrDefault()?.CowId ?? 0,
                HeatDate = DateTime.Today,
                BreedDate = DateTime.Today,
                PregDate = DateTime.Today,
                ExpDate = DateTime.Today,
                DateCalved = DateTime.Today
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

        return View(new BreedPageViewModel { Reports = list, Cows = cows, Draft = draft });
    }

    [HttpGet]
    public async Task<IActionResult> EditModal(int id)
    {
        var draft = await _context.BreedReports.AsNoTracking().FirstOrDefaultAsync(r => r.Brid == id)
            ?? new BreedReport { HeatDate = DateTime.Today, BreedDate = DateTime.Today, PregDate = DateTime.Today, ExpDate = DateTime.Today, DateCalved = DateTime.Today };
        var cows = await _context.Cows.AsNoTracking().Where(c => c.IsActive || c.CowId == draft.CowId).OrderBy(c => c.CowName).ToListAsync();
        return PartialView("~/Views/Breeding/_EditModal.cshtml", new BreedPageViewModel { Draft = draft, Cows = cows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind("Brid,CowId,CowName,HeatDate,BreedDate,PregDate,ExpDate,DateCalved,CowAge,Remarks", Prefix = "Draft")] BreedReport draft)
    {
        var cow = await _context.Cows.FindAsync(draft.CowId);
        if (cow == null)
        {
            ModelState.AddModelError(nameof(draft.CowId), "Cow select karen.");
        }

        if (!ModelState.IsValid)
        {
            var cows = await _context.Cows.AsNoTracking().OrderBy(c => c.CowName).ToListAsync();
            var list = await _context.BreedReports.AsNoTracking().OrderByDescending(r => r.BreedDate).ToListAsync();
            return View("Index", new BreedPageViewModel { Reports = list, Cows = cows, Draft = draft });
        }

        draft.CowName = cow!.CowName;
        if (draft.Brid == 0)
        {
            _context.BreedReports.Add(draft);
        }
        else
        {
            _context.BreedReports.Update(draft);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.BreedReports.FindAsync(id);
        if (entity != null)
        {
            _context.BreedReports.Remove(entity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
