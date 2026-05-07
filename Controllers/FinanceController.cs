using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class FinanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public FinanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? employeeId)
    {
        var employees = await _context.Employees.AsNoTracking().OrderBy(e => e.EmpId).ToListAsync();
        if (employees.Count == 0)
        {
            return View(new FinanceDashboardModel { Employees = employees });
        }

        var filter = employeeId ?? employees.FirstOrDefault()?.EmpId;

        var expQuery = _context.ExpenditureEntries.AsNoTracking().AsQueryable();
        var incQuery = _context.IncomeEntries.AsNoTracking().AsQueryable();
        if (filter.HasValue)
        {
            expQuery = expQuery.Where(x => x.EmpId == filter.Value);
            incQuery = incQuery.Where(x => x.EmpId == filter.Value);
        }

        var model = new FinanceDashboardModel
        {
            EmployeeId = filter ?? 0,
            FilterEmployeeId = filter,
            Employees = employees,
            Expenditures = await expQuery.OrderByDescending(x => x.ExpDate).ToListAsync(),
            Incomes = await incQuery.OrderByDescending(x => x.IncDate).ToListAsync()
        };

        return View(model);
    }

    // Return expenditures partial for AJAX
    public async Task<IActionResult> ExpendituresPartial(int? employeeId)
    {
        var filter = employeeId;
        var expQuery = _context.ExpenditureEntries.AsNoTracking().AsQueryable();
        if (filter.HasValue)
            expQuery = expQuery.Where(x => x.EmpId == filter.Value);

        var list = await expQuery.OrderByDescending(x => x.ExpDate).ToListAsync();
        ViewBag.EmployeeId = filter ?? 0;
        return PartialView("_ExpendituresPartial", list);
    }

    // Return incomes partial for AJAX
    public async Task<IActionResult> IncomesPartial(int? employeeId)
    {
        var filter = employeeId;
        var incQuery = _context.IncomeEntries.AsNoTracking().AsQueryable();
        if (filter.HasValue)
            incQuery = incQuery.Where(x => x.EmpId == filter.Value);

        var list = await incQuery.OrderByDescending(x => x.IncDate).ToListAsync();
        ViewBag.EmployeeId = filter ?? 0;
        return PartialView("_IncomesPartial", list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpenditure([Bind("ExpDate,ExpPurpose,ExpAmount,EmpId")] ExpenditureEntry row)
    {
        _context.ExpenditureEntries.Add(row);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { employeeId = row.EmpId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddIncome([Bind("IncDate,IncPurpose,IncAmt,EmpId")] IncomeEntry row)
    {
        _context.IncomeEntries.Add(row);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { employeeId = row.EmpId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpenditure(int id, int empId)
    {
        var entity = await _context.ExpenditureEntries.FindAsync(id);
        if (entity != null)
        {
            _context.ExpenditureEntries.Remove(entity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { employeeId = empId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIncome(int id, int empId)
    {
        var entity = await _context.IncomeEntries.FindAsync(id);
        if (entity != null)
        {
            _context.IncomeEntries.Remove(entity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { employeeId = empId });
    }
}
