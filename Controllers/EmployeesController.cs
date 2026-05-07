using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id)
    {
        var list = await _context.Employees.AsNoTracking().OrderBy(e => e.EmpId).ToListAsync();
        Employee draft;
        if (id.HasValue)
        {
            draft = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmpId == id.Value) ?? new Employee();
        }
        else
        {
            draft = new Employee { EmpDob = DateTime.Today.AddYears(-25) };
        }

        return View(new EmployeePageViewModel { Employees = list, Draft = draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind("EmpId,EmpName,EmpDob,Gender,Phone,Address,EmpPass", Prefix = "Draft")] Employee draft)
    {
        if (draft.EmpId == 0)
        {
            _context.Employees.Add(draft);
        }
        else
        {
            var existing = await _context.Employees.FindAsync(draft.EmpId);
            if (existing == null)
            {
                return NotFound();
            }

            existing.EmpName = draft.EmpName;
            existing.EmpDob = draft.EmpDob;
            existing.Gender = draft.Gender;
            existing.Phone = draft.Phone;
            existing.Address = draft.Address;
            if (!string.IsNullOrWhiteSpace(draft.EmpPass))
            {
                existing.EmpPass = draft.EmpPass;
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _context.Employees.FindAsync(id);
            if (entity != null)
            {
                _context.Employees.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Is employee par milk sales / finance records linked hain.";
        }

        return RedirectToAction(nameof(Index));
    }
}
