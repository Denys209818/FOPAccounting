using ClosedXML.Excel;
using FopAccounting.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Controllers;

public class ReportsController : BaseController
{
    public ReportsController(AppDbContext db) : base(db) { }

    public IActionResult Index(string? type, string? search, string? sort)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");

        FillLayoutData();

        int profileId = CurrentProfileId!.Value;

        var query = BuildQuery(profileId, type, search, sort);

        ViewBag.Type = type;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.TotalIn = Db.StockOperations
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Product.ProfileId == profileId && x.OperationType == "Надходження")
            .Sum(x => (decimal?)(x.Quantity * (x.Product!.PurchasePrice ?? 0))) ?? 0;
        ViewBag.TotalOut = Db.StockOperations
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Product.ProfileId == profileId &&
                        (x.OperationType == "Продаж" || x.OperationType == "Списання"))
            .Sum(x => (decimal?)(x.Quantity * (x.Product!.PurchasePrice ?? 0))) ?? 0;

        return View(query.ToList());
    }

    public IActionResult ExportXlsx(string? type, string? search, string? sort)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index", "Profiles");

        int profileId = CurrentProfileId!.Value;
        var rows = BuildQuery(profileId, type, search, sort).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Звіти складу");

        string[] headers = ["Дата", "Документ", "Товар", "Контрагент", "Кількість", "Сума (грн)", "Коментар"];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F6BED");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.Date.ToString("dd.MM.yyyy HH:mm");
            ws.Cell(row, 2).Value = r.Type;
            ws.Cell(row, 3).Value = r.ProductName;
            ws.Cell(row, 4).Value = r.Counterparty;
            ws.Cell(row, 5).Value = r.Quantity;
            ws.Cell(row, 6).Value = r.Amount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = r.Comment ?? "";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        string fileName = $"звіт_{DateTime.Now:yyyy-MM-dd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private IQueryable<FopAccounting.Models.StockOperation> BuildQuery(int profileId, string? type, string? search, string? sort)
    {
        var query = Db.StockOperations
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Product.ProfileId == profileId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.OperationType == type);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x =>
                (x.Product != null && x.Product.Name.Contains(search)) ||
                (x.Comment != null && x.Comment.Contains(search)));
        }

        return sort switch
        {
            "date" => query.OrderBy(x => x.CreatedAt),
            "amount_desc" => query.OrderByDescending(x => x.Quantity * (x.Product!.PurchasePrice ?? 0)),
            "qty_desc" => query.OrderByDescending(x => x.Quantity),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
