using FopAccounting.Data;
using FopAccounting.Models;
using FopAccounting.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Controllers;

public class ProductsController : BaseController
{
    public ProductsController(AppDbContext db) : base(db) { }

    public IActionResult Index(string? search, string? category, string? warehouse, string? sort, bool lowStockOnly = false)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");

        FillLayoutData();

        int profileId = CurrentProfileId!.Value;

        var query = Db.Products
            .Where(x => x.ProfileId == profileId)
            .AsQueryable();

        var all = query.ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Article != null && x.Article.Contains(search)) ||
                (x.Category != null && x.Category.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);

        if (!string.IsNullOrWhiteSpace(warehouse) && warehouse != "Основний склад")
            query = query.Where(x => false);

        if (lowStockOnly)
            query = query.Where(x => x.Quantity <= x.MinStock);

        query = sort switch
        {
            "name" => query.OrderBy(x => x.Name),
            "quantity" => query.OrderBy(x => x.Quantity),
            "quantity_desc" => query.OrderByDescending(x => x.Quantity),
            "price" => query.OrderBy(x => x.SalePrice),
            "price_desc" => query.OrderByDescending(x => x.SalePrice),
            "margin_desc" => query.OrderByDescending(x => (x.SalePrice ?? 0) - (x.PurchasePrice ?? 0)),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var products = query.ToList();

        ViewBag.CanEdit = CanEditProducts();

        return View(new ProductListViewModel
        {
            Products = products,
            Search = search,
            Category = category,
            Warehouse = warehouse,
            Sort = sort,
            LowStockOnly = lowStockOnly,
            Categories = all
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => x.Category!)
                .Distinct()
                .OrderBy(x => x)
                .ToList(),
            Warehouses = new List<string> { "Основний склад" },
            TotalQuantity = all.Sum(x => x.Quantity),
            TotalCost = all.Sum(x => x.Quantity * (x.PurchasePrice ?? 0)),
            PotentialIncome = all.Sum(x => x.Quantity * (x.SalePrice ?? 0))
        });
    }

    public IActionResult Create()
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanEditProducts()) return RedirectToAction("Index");

        FillLayoutData();

        return View(new Product
        {
            Article = "SKU-" + DateTime.Now.ToString("HHmmss"),
            MinStock = 5,
            Quantity = 0
        });
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanEditProducts()) return RedirectToAction("Index");

        product.Id = 0;
        product.ProfileId = CurrentProfileId!.Value;
        product.CreatedAt = DateTime.Now;

        Db.Products.Add(product);
        Db.SaveChanges();

        if (product.Quantity > 0)
        {
            Db.StockOperations.Add(new StockOperation
            {
                ProductId = product.Id,
                OperationType = "Надходження",
                Quantity = product.Quantity,
                Comment = "Початковий залишок товару",
                CreatedAt = DateTime.Now
            });
            Db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanEditProducts()) return RedirectToAction("Index");

        FillLayoutData();

        var product = Db.Products
            .FirstOrDefault(x => x.Id == id && x.ProfileId == CurrentProfileId!.Value);

        return product == null ? RedirectToAction("Index") : View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product model)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanEditProducts()) return RedirectToAction("Index");

        var product = Db.Products
            .FirstOrDefault(x => x.Id == model.Id && x.ProfileId == CurrentProfileId!.Value);

        if (product == null)
            return RedirectToAction("Index");

        int oldQuantity = product.Quantity;

        product.Article = model.Article;
        product.Name = model.Name;
        product.Category = model.Category;
        product.Quantity = model.Quantity;
        product.PurchasePrice = model.PurchasePrice;
        product.SalePrice = model.SalePrice;
        product.MinStock = model.MinStock;

        Db.SaveChanges();

        int diff = model.Quantity - oldQuantity;
        if (diff != 0)
        {
            Db.StockOperations.Add(new StockOperation
            {
                ProductId = product.Id,
                OperationType = diff > 0 ? "Надходження" : "Списання",
                Quantity = Math.Abs(diff),
                Comment = "Коригування залишку після редагування товару",
                CreatedAt = DateTime.Now
            });
            Db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");
        if (!CanEditProducts()) return RedirectToAction("Index");

        var product = Db.Products
            .FirstOrDefault(x => x.Id == id && x.ProfileId == CurrentProfileId!.Value);

        if (product != null)
        {
            Db.Products.Remove(product);
            Db.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
