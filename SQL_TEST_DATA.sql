-- ============================================================
-- Тестові дані для демонстраційного відео
-- Пароль зберігається як plain-text (підтримується AuthController)
-- Логін: demo@fop.ua  |  Пароль: demo123
-- ============================================================

-- Очищення (порядок важливий через FK)
DELETE FROM ManagementOperations;
DELETE FROM StockOperations;
DELETE FROM Products;
DELETE FROM UserProfileAccess;
DELETE FROM BusinessProfiles;
DELETE FROM Users;
DELETE FROM Roles;

-- ============================================================
-- Ролі
-- ============================================================
INSERT INTO Roles (RoleName) VALUES (N'Owner');
INSERT INTO Roles (RoleName) VALUES (N'Manager');

-- ============================================================
-- Користувач (пароль plain-text для тесту)
-- ============================================================
INSERT INTO Users (FullName, Email, PasswordHash, CreatedAt)
VALUES (N'Іваненко Денис Олексійович', N'demo@fop.ua', N'demo123', GETDATE());

-- ============================================================
-- Бізнес-профіль
-- ============================================================
INSERT INTO BusinessProfiles (ProfileName, BusinessType, OwnerId, TaxNumber, Phone, Address, Description, CreatedAt)
VALUES (
    N'ФОП Іваненко Д.О.',
    N'Роздрібна торгівля',
    (SELECT Id FROM Users WHERE Email = N'demo@fop.ua'),
    N'3456789012',
    N'+380671234567',
    N'м. Київ, вул. Хрещатик, 1',
    N'Продаж електроніки та аксесуарів',
    GETDATE()
);

-- ============================================================
-- Доступ власника до профілю
-- ============================================================
INSERT INTO UserProfileAccess (UserId, ProfileId, RoleId)
VALUES (
    (SELECT Id FROM Users WHERE Email = N'demo@fop.ua'),
    (SELECT Id FROM BusinessProfiles WHERE TaxNumber = N'3456789012'),
    (SELECT Id FROM Roles WHERE RoleName = N'Owner')
);

-- ============================================================
-- Товари
-- ============================================================
DECLARE @ProfileId INT = (SELECT Id FROM BusinessProfiles WHERE TaxNumber = N'3456789012');

INSERT INTO Products (ProfileId, Name, Category, Article, Quantity, PurchasePrice, SalePrice, MinStock, CreatedAt) VALUES
(@ProfileId, N'Навушники Sony WH-1000XM5',  N'Аудіо',       N'SKU-001', 12,  6500.00,  9200.00,  3, DATEADD(day, -30, GETDATE())),
(@ProfileId, N'Навушники JBL Tune 510BT',   N'Аудіо',       N'SKU-002', 4,   1200.00,  1900.00,  5, DATEADD(day, -28, GETDATE())),
(@ProfileId, N'Смартфон Samsung Galaxy A55',N'Смартфони',   N'SKU-003', 8,  10500.00, 14900.00,  2, DATEADD(day, -25, GETDATE())),
(@ProfileId, N'Смартфон Xiaomi Redmi 13',   N'Смартфони',   N'SKU-004', 15,  4200.00,  6500.00,  5, DATEADD(day, -22, GETDATE())),
(@ProfileId, N'Павербанк Anker 20000 mAh',  N'Аксесуари',   N'SKU-005', 20,   800.00,  1350.00,  8, DATEADD(day, -20, GETDATE())),
(@ProfileId, N'Кабель USB-C 2m Baseus',     N'Аксесуари',   N'SKU-006', 50,    85.00,   160.00, 15, DATEADD(day, -18, GETDATE())),
(@ProfileId, N'Чохол для iPhone 15 Pro',    N'Аксесуари',   N'SKU-007', 3,    120.00,   280.00, 10, DATEADD(day, -15, GETDATE())),
(@ProfileId, N'Ноутбук ASUS VivoBook 15',   N'Ноутбуки',    N'SKU-008', 5,  18000.00, 24500.00,  2, DATEADD(day, -10, GETDATE()));

-- ============================================================
-- Складські операції
-- ============================================================

-- Надходження при створенні (початкові залишки)
INSERT INTO StockOperations (ProductId, OperationType, Quantity, Comment, CreatedAt)
SELECT Id, N'Надходження', Quantity, N'Початковий залишок товару', DATEADD(day, -30, GETDATE())
FROM Products WHERE ProfileId = @ProfileId;

-- Продажі
INSERT INTO StockOperations (ProductId, OperationType, Quantity, Comment, CreatedAt) VALUES
((SELECT Id FROM Products WHERE Article = N'SKU-001' AND ProfileId = @ProfileId), N'Продаж', 3, N'Замовлення #1023', DATEADD(day, -20, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-003' AND ProfileId = @ProfileId), N'Продаж', 2, N'Замовлення #1024', DATEADD(day, -18, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-004' AND ProfileId = @ProfileId), N'Продаж', 5, N'Замовлення #1025', DATEADD(day, -15, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-005' AND ProfileId = @ProfileId), N'Продаж', 8, N'Замовлення #1026', DATEADD(day, -12, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-006' AND ProfileId = @ProfileId), N'Продаж', 20, N'Замовлення #1027', DATEADD(day, -10, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-002' AND ProfileId = @ProfileId), N'Продаж', 6, N'Замовлення #1028', DATEADD(day,  -7, GETDATE()));

-- Додаткове надходження
INSERT INTO StockOperations (ProductId, OperationType, Quantity, Comment, CreatedAt) VALUES
((SELECT Id FROM Products WHERE Article = N'SKU-006' AND ProfileId = @ProfileId), N'Надходження', 20, N'Поповнення від постачальника', DATEADD(day, -5, GETDATE())),
((SELECT Id FROM Products WHERE Article = N'SKU-007' AND ProfileId = @ProfileId), N'Надходження', 10, N'Нова партія', DATEADD(day, -3, GETDATE()));

-- Списання браку
INSERT INTO StockOperations (ProductId, OperationType, Quantity, Comment, CreatedAt) VALUES
((SELECT Id FROM Products WHERE Article = N'SKU-002' AND ProfileId = @ProfileId), N'Списання', 2, N'Бракований товар', DATEADD(day, -8, GETDATE()));

-- ============================================================
-- Фінансові операції
-- ============================================================
INSERT INTO ManagementOperations (ProfileId, OperationType, Amount, Description, CreatedAt) VALUES
(@ProfileId, N'Дохід',    27600.00, N'Продаж смартфонів за травень',          DATEADD(day, -20, GETDATE())),
(@ProfileId, N'Дохід',    11400.00, N'Продаж навушників за травень',          DATEADD(day, -18, GETDATE())),
(@ProfileId, N'Витрата',  52500.00, N'Закупівля товару у постачальника',       DATEADD(day, -15, GETDATE())),
(@ProfileId, N'Дохід',    32500.00, N'Продаж ноутбуків — 2 шт.',              DATEADD(day, -12, GETDATE())),
(@ProfileId, N'Витрата',   3200.00, N'Оренда складу за місяць',               DATEADD(day, -10, GETDATE())),
(@ProfileId, N'Дохід',    10800.00, N'Продаж аксесуарів за травень',          DATEADD(day,  -8, GETDATE())),
(@ProfileId, N'Витрата',   1500.00, N'Реклама в соціальних мережах',          DATEADD(day,  -6, GETDATE())),
(@ProfileId, N'Дохід',    18600.00, N'Оптовий продаж Xiaomi Redmi 13 — 3 шт', DATEADD(day,  -4, GETDATE())),
(@ProfileId, N'Витрата',    750.00, N'Пальне та логістика',                   DATEADD(day,  -2, GETDATE())),
(@ProfileId, N'Дохід',     4800.00, N'Продаж павербанків — 4 шт.',            DATEADD(day,  -1, GETDATE()));

-- ============================================================
-- Підсумок:
--   Доходи:  105,700 грн
--   Витрати:  57,950 грн
--   Баланс:   47,750 грн
-- ============================================================
