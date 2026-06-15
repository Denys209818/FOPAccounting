using System.Security.Cryptography;
using System.Text;
using FopAccounting.Data;
using Microsoft.AspNetCore.Mvc;

namespace FopAccounting.Controllers;

public class AuthController : BaseController
{
    public AuthController(AppDbContext db) : base(db) { }

    public IActionResult Login()
    {
        if (IsAuthorized())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        email = (email ?? string.Empty).Trim();
        password = password ?? string.Empty;

        var user = Db.Users.FirstOrDefault(x => x.Email == email);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            ViewBag.Error = "Неправильний email або пароль";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);

        var firstAccess = Db.UserProfileAccesses
            .FirstOrDefault(x => x.UserId == user.Id);

        if (firstAccess != null)
            HttpContext.Session.SetInt32("ProfileId", firstAccess.ProfileId);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (IsAuthorized())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public IActionResult Register(string fullName, string email, string password, string confirmPassword)
    {
        fullName = (fullName ?? string.Empty).Trim();
        email = (email ?? string.Empty).Trim();
        password = password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Заповніть усі поля";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Паролі не збігаються";
            return View();
        }

        if (Db.Users.Any(x => x.Email == email))
        {
            ViewBag.Error = "Користувач з таким email вже існує";
            return View();
        }

        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToHexString(bytes).ToLowerInvariant();

        var user = new FopAccounting.Models.User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = hash,
            CreatedAt = DateTime.Now
        };

        Db.Users.Add(user);
        Db.SaveChanges();

        HttpContext.Session.SetInt32("UserId", user.Id);

        return RedirectToAction("Index", "Profiles");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private static bool VerifyPassword(string password, string storedPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(storedPasswordHash))
            return false;

        // Підтримка простого тестового пароля з SQL_TEST_DATA.sql.
        if (storedPasswordHash == password)
            return true;

        // Підтримка SHA256, якщо пізніше пароль буде збережено як хеш.
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToHexString(bytes).ToLowerInvariant();

        return storedPasswordHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
