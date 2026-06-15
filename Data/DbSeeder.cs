using System.Security.Cryptography;
using System.Text;
using FopAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Roles.Any()) return;

        var now = DateTime.UtcNow;

        // Ролі
        var roleOwner   = new Role { RoleName = "Owner" };
        var roleManager = new Role { RoleName = "Manager" };
        db.Roles.AddRange(roleOwner, roleManager);
        db.SaveChanges();

        // Користувач  demo@fop.ua / demo123
        var user = new User
        {
            FullName     = "Іваненко Денис Олексійович",
            Email        = "demo@fop.ua",
            PasswordHash = Sha256("demo123"),
            CreatedAt    = now.AddDays(-30)
        };
        db.Users.Add(user);
        db.SaveChanges();

        // Бізнес-профіль
        var profile = new BusinessProfile
        {
            ProfileName  = "ФОП Іваненко Д.О.",
            BusinessType = "Роздрібна торгівля",
            OwnerId      = user.Id,
            TaxNumber    = "3456789012",
            Phone        = "+380671234567",
            Address      = "м. Київ, вул. Хрещатик, 1",
            Description  = "Продаж електроніки та аксесуарів",
            CreatedAt    = now.AddDays(-30)
        };
        db.BusinessProfiles.Add(profile);
        db.SaveChanges();

        // Доступ власника
        db.UserProfileAccesses.Add(new UserProfileAccess
        {
            UserId    = user.Id,
            ProfileId = profile.Id,
            RoleId    = roleOwner.Id
        });
        db.SaveChanges();

        // Товари
        var products = new[]
        {
            new Product { ProfileId = profile.Id, Name = "Навушники Sony WH-1000XM5",   Category = "Аудіо",      Article = "SKU-001", Quantity = 12, PurchasePrice = 6500m,  SalePrice = 9200m,  MinStock = 3,  CreatedAt = now.AddDays(-30) },
            new Product { ProfileId = profile.Id, Name = "Навушники JBL Tune 510BT",    Category = "Аудіо",      Article = "SKU-002", Quantity = 4,  PurchasePrice = 1200m,  SalePrice = 1900m,  MinStock = 5,  CreatedAt = now.AddDays(-28) },
            new Product { ProfileId = profile.Id, Name = "Смартфон Samsung Galaxy A55", Category = "Смартфони",  Article = "SKU-003", Quantity = 8,  PurchasePrice = 10500m, SalePrice = 14900m, MinStock = 2,  CreatedAt = now.AddDays(-25) },
            new Product { ProfileId = profile.Id, Name = "Смартфон Xiaomi Redmi 13",    Category = "Смартфони",  Article = "SKU-004", Quantity = 15, PurchasePrice = 4200m,  SalePrice = 6500m,  MinStock = 5,  CreatedAt = now.AddDays(-22) },
            new Product { ProfileId = profile.Id, Name = "Павербанк Anker 20000 mAh",   Category = "Аксесуари",  Article = "SKU-005", Quantity = 20, PurchasePrice = 800m,   SalePrice = 1350m,  MinStock = 8,  CreatedAt = now.AddDays(-20) },
            new Product { ProfileId = profile.Id, Name = "Кабель USB-C 2m Baseus",      Category = "Аксесуари",  Article = "SKU-006", Quantity = 50, PurchasePrice = 85m,    SalePrice = 160m,   MinStock = 15, CreatedAt = now.AddDays(-18) },
            new Product { ProfileId = profile.Id, Name = "Чохол для iPhone 15 Pro",     Category = "Аксесуари",  Article = "SKU-007", Quantity = 3,  PurchasePrice = 120m,   SalePrice = 280m,   MinStock = 10, CreatedAt = now.AddDays(-15) },
            new Product { ProfileId = profile.Id, Name = "Ноутбук ASUS VivoBook 15",    Category = "Ноутбуки",   Article = "SKU-008", Quantity = 5,  PurchasePrice = 18000m, SalePrice = 24500m, MinStock = 2,  CreatedAt = now.AddDays(-10) },
        };
        db.Products.AddRange(products);
        db.SaveChanges();

        var p = products.ToDictionary(x => x.Article!);

        // Складські операції — початкові залишки
        var stockOps = products.Select(prod => new StockOperation
        {
            ProductId     = prod.Id,
            OperationType = "Надходження",
            Quantity      = prod.Quantity,
            Comment       = "Початковий залишок товару",
            CreatedAt     = now.AddDays(-30)
        }).ToList();

        // Продажі
        stockOps.AddRange(new[]
        {
            new StockOperation { ProductId = p["SKU-001"].Id, OperationType = "Продаж",    Quantity = 3,  Comment = "Замовлення #1023",              CreatedAt = now.AddDays(-20) },
            new StockOperation { ProductId = p["SKU-003"].Id, OperationType = "Продаж",    Quantity = 2,  Comment = "Замовлення #1024",              CreatedAt = now.AddDays(-18) },
            new StockOperation { ProductId = p["SKU-004"].Id, OperationType = "Продаж",    Quantity = 5,  Comment = "Замовлення #1025",              CreatedAt = now.AddDays(-15) },
            new StockOperation { ProductId = p["SKU-005"].Id, OperationType = "Продаж",    Quantity = 8,  Comment = "Замовлення #1026",              CreatedAt = now.AddDays(-12) },
            new StockOperation { ProductId = p["SKU-006"].Id, OperationType = "Продаж",    Quantity = 20, Comment = "Замовлення #1027",              CreatedAt = now.AddDays(-10) },
            new StockOperation { ProductId = p["SKU-002"].Id, OperationType = "Продаж",    Quantity = 6,  Comment = "Замовлення #1028",              CreatedAt = now.AddDays(-7)  },
            new StockOperation { ProductId = p["SKU-006"].Id, OperationType = "Надходження",Quantity = 20, Comment = "Поповнення від постачальника",  CreatedAt = now.AddDays(-5)  },
            new StockOperation { ProductId = p["SKU-007"].Id, OperationType = "Надходження",Quantity = 10, Comment = "Нова партія",                  CreatedAt = now.AddDays(-3)  },
            new StockOperation { ProductId = p["SKU-002"].Id, OperationType = "Списання",  Quantity = 2,  Comment = "Бракований товар",              CreatedAt = now.AddDays(-8)  },
        });
        db.StockOperations.AddRange(stockOps);

        // Фінансові операції
        db.ManagementOperations.AddRange(new[]
        {
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 27600m, Description = "Продаж смартфонів за травень",           CreatedAt = now.AddDays(-20) },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 11400m, Description = "Продаж навушників за травень",           CreatedAt = now.AddDays(-18) },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Витрата",  Amount = 52500m, Description = "Закупівля товару у постачальника",        CreatedAt = now.AddDays(-15) },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 32500m, Description = "Продаж ноутбуків — 2 шт.",               CreatedAt = now.AddDays(-12) },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Витрата",  Amount = 3200m,  Description = "Оренда складу за місяць",                CreatedAt = now.AddDays(-10) },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 10800m, Description = "Продаж аксесуарів за травень",           CreatedAt = now.AddDays(-8)  },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Витрата",  Amount = 1500m,  Description = "Реклама в соціальних мережах",           CreatedAt = now.AddDays(-6)  },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 18600m, Description = "Оптовий продаж Xiaomi Redmi 13 — 3 шт", CreatedAt = now.AddDays(-4)  },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Витрата",  Amount = 750m,   Description = "Пальне та логістика",                    CreatedAt = now.AddDays(-2)  },
            new ManagementOperation { ProfileId = profile.Id, OperationType = "Дохід",    Amount = 4800m,  Description = "Продаж павербанків — 4 шт.",              CreatedAt = now.AddDays(-1)  },
        });

        db.SaveChanges();
    }

    private static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
