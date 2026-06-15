using FopAccounting.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Controllers;

public class HomeController : BaseController
{
    public HomeController(AppDbContext db) : base(db) { }

    public IActionResult Index()
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");

        FillLayoutData();

        int profileId = CurrentProfileId!.Value;

        var products = Db.Products
            .Where(x => x.ProfileId == profileId)
            .ToList();

        var operations = Db.ManagementOperations
            .Where(x => x.ProfileId == profileId)
            .ToList();

        ViewBag.ProductsCount = products.Count;
        ViewBag.TotalQuantity = products.Sum(x => x.Quantity);
        ViewBag.TotalValue = products.Sum(x => x.Quantity * (x.PurchasePrice ?? 0));
        ViewBag.LowStock = products.Count(x => x.Quantity <= x.MinStock);
        ViewBag.Income = operations.Where(x => x.OperationType == "Дохід").Sum(x => x.Amount);
        ViewBag.Expense = operations.Where(x => x.OperationType == "Витрата").Sum(x => x.Amount);

        ViewBag.RecentStock = Db.StockOperations
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Product.ProfileId == profileId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToList();

        return View();
    }
}
