using FopAccounting.Data;
using FopAccounting.Models;
using Microsoft.AspNetCore.Mvc;

namespace FopAccounting.Controllers;

public class FinanceController : BaseController
{
    public FinanceController(AppDbContext db) : base(db) { }

    public IActionResult Index(string? type)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");

        FillLayoutData();

        int profileId = CurrentProfileId!.Value;

        var query = Db.ManagementOperations
            .Where(x => x.ProfileId == profileId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.OperationType == type);

        ViewBag.Type = type;
        ViewBag.CanManageFinance = CanManageFinance();
        ViewBag.Income = Db.ManagementOperations
            .Where(x => x.ProfileId == profileId && x.OperationType == "Дохід")
            .Sum(x => (decimal?)x.Amount) ?? 0;
        ViewBag.Expense = Db.ManagementOperations
            .Where(x => x.ProfileId == profileId && x.OperationType == "Витрата")
            .Sum(x => (decimal?)x.Amount) ?? 0;

        return View(query.OrderByDescending(x => x.CreatedAt).ToList());
    }

    [HttpPost]
    public IActionResult Create(ManagementOperation op)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanManageFinance()) return RedirectToAction("Index");

        op.Id = 0;
        op.ProfileId = CurrentProfileId!.Value;
        op.CreatedAt = DateTime.Now;

        if (string.IsNullOrWhiteSpace(op.OperationType))
            op.OperationType = "Дохід";

        Db.ManagementOperations.Add(op);
        Db.SaveChanges();

        return RedirectToAction("Index");
    }
}
