# FopAccounting

Веб-застосунок для обліку фінансів, товарів і складських операцій фізичних осіб–підприємців (ФОП).

## Зміст

- [Опис](#опис)
- [Функціонал](#функціонал)
- [Технічний стек](#технічний-стек)
- [Локальний запуск](#локальний-запуск)
- [Запуск через Docker](#запуск-через-docker)
- [Деплой на Render](#деплой-на-render)
- [Структура проекту](#структура-проекту)

---

## Опис

FopAccounting — це система управлінського обліку для ФОП. Дозволяє вести кілька бізнес-профілів, контролювати залишки товарів на складі, фіксувати фінансові операції та отримувати звіти у форматі Excel.

---

## Функціонал

- **Автентифікація** — реєстрація та вхід в обліковий запис
- **Бізнес-профілі** — кілька профілів на одного користувача (наприклад, різні ФОП або торгові точки)
- **Ролева модель доступу** — власник може надавати іншим користувачам доступ до свого профілю з певною роллю
- **Товари** — каталог товарів з артикулом, категорією, ціною закупівлі та продажу, мінімальним залишком
- **Складські операції** — надходження, продаж, списання товарів
- **Фінансові операції** — облік доходів і витрат
- **Звіти** — перегляд складських операцій з фільтрацією, пошуком і сортуванням; експорт у `.xlsx`
- **Дашборд** — зведена статистика: кількість товарів, загальна вартість складу, низькі залишки, баланс

---

## Технічний стек

| Шар | Технологія |
|---|---|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| База даних | PostgreSQL 16 |
| Звіти | ClosedXML |
| Контейнеризація | Docker |
| Хостинг | Render |

---

## Локальний запуск

### Вимоги

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16](https://www.postgresql.org/download/) (або через Docker: `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16`)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Кроки

```bash
# 1. Клонуй репозиторій
git clone https://github.com/ТВІ_НІКНЕЙМ/fop-accounting.git
cd fop-accounting

# 2. Відновити залежності
dotnet restore

# 3. Застосувати міграції (база буде створена автоматично)
dotnet ef database update

# 4. Запустити проект
dotnet run
```

Застосунок буде доступний за адресою: `https://localhost:51812`

> Рядок підключення знаходиться в [appsettings.json](appsettings.json). За замовчуванням: `Host=localhost;Port=5432;Database=FopAccountingDB;Username=postgres;Password=postgres`

---

## Запуск через Docker

```bash
# Зібрати образ
docker build -t fop-accounting .

# Запустити контейнер
docker run -p 8080:8080 \
  -e DATABASE_URL="postgresql://postgres:postgres@host.docker.internal:5432/FopAccountingDB" \
  fop-accounting
```

Застосунок буде доступний за адресою: `http://localhost:8080`

---

## Деплой на Render

### 1. Створи PostgreSQL на Render

**New → PostgreSQL** → Plan: Free → **Create Database**

Скопіюй **Internal Database URL**.

### 2. Створи Web Service

**New → Web Service** → підключи репозиторій GitHub

| Параметр | Значення |
|---|---|
| Runtime | Docker |
| Port | `8080` |

**Environment Variables:**

| Ключ | Значення |
|---|---|
| `DATABASE_URL` | Internal URL з попереднього кроку |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

Натисни **Create Web Service** — деплой відбудеться автоматично. Міграції застосовуються при кожному запуску.

---

## Структура проекту

```
fop_accounting_project/
├── Controllers/        # MVC контролери
│   ├── AuthController.cs
│   ├── HomeController.cs
│   ├── ProductsController.cs
│   ├── FinanceController.cs
│   ├── ProfilesController.cs
│   └── ReportsController.cs
├── Models/             # Моделі даних
├── Views/              # Razor шаблони
├── Data/
│   └── AppDbContext.cs # EF Core контекст
├── Migrations/         # EF Core міграції
├── wwwroot/            # Статичні файли (CSS)
├── Dockerfile
└── Program.cs
```

---

## Автор

Дипломний проект — Факультет прикладної математики, КПІ ім. Ігоря Сікорського.
