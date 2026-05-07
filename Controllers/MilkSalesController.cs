using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class MilkSalesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MilkSalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id)
    {
        var employees = await _context.Employees.AsNoTracking().OrderBy(e => e.EmpName).ToListAsync();
        var list = await _context.MilkSales.AsNoTracking()
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.SId)
            .ToListAsync();

        MilkSale draft;
        if (id.HasValue)
        {
            draft = await _context.MilkSales.AsNoTracking().FirstOrDefaultAsync(s => s.SId == id.Value)
                ?? new MilkSale { Date = DateTime.Today, EmpId = employees.FirstOrDefault()?.EmpId ?? 0 };
        }
        else
        {
            draft = new MilkSale
            {
                Date = DateTime.Today,
                EmpId = employees.FirstOrDefault()?.EmpId ?? 0
            };
        }

        return View(new MilkSalesPageViewModel { Sales = list, Employees = employees, Draft = draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind("SId,Date,Uprice,ClientName,ClientPhone,EmpId,Quantity", Prefix = "Draft")] MilkSale draft)
    {
        if (!await _context.Employees.AnyAsync(e => e.EmpId == draft.EmpId))
        {
            ModelState.AddModelError(nameof(draft.EmpId), "Employee select karen.");
        }

        if (!ModelState.IsValid)
        {
            var employees = await _context.Employees.AsNoTracking().OrderBy(e => e.EmpName).ToListAsync();
            var list = await _context.MilkSales.AsNoTracking().OrderByDescending(s => s.Date).ToListAsync();
            return View("Index", new MilkSalesPageViewModel { Sales = list, Employees = employees, Draft = draft });
        }

        if (draft.SId == 0)
        {
            _context.MilkSales.Add(draft);
        }
        else
        {
            _context.MilkSales.Update(draft);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.MilkSales.FindAsync(id);
        if (entity != null)
        {
            _context.MilkSales.Remove(entity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
