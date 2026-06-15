using FopAccounting.Data;
using FopAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Controllers;

public class BaseController : Controller
{
    protected readonly AppDbContext Db;

    public BaseController(AppDbContext db)
    {
        Db = db;
    }

    protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
    protected int? CurrentProfileId => HttpContext.Session.GetInt32("ProfileId");

    protected User? CurrentUser => CurrentUserId == null
        ? null
        : Db.Users.FirstOrDefault(x => x.Id == CurrentUserId.Value);

    protected BusinessProfile? CurrentProfile => CurrentProfileId == null
        ? null
        : Db.BusinessProfiles.FirstOrDefault(x => x.Id == CurrentProfileId.Value);

    protected bool IsAuthorized()
    {
        return CurrentUserId != null;
    }

    protected IActionResult RedirectToLogin()
    {
        return RedirectToAction("Login", "Auth");
    }

    protected void FillLayoutData()
    {
        ViewBag.LayoutUser = CurrentUser;
        ViewBag.LayoutProfile = CurrentProfile;
        ViewBag.LayoutRole = CurrentRoleName();
    }

    protected UserProfileAccess? CurrentAccess()
    {
        if (CurrentUserId == null || CurrentProfileId == null)
            return null;

        return Db.UserProfileAccesses
            .Include(x => x.Role)
            .FirstOrDefault(x => x.UserId == CurrentUserId.Value && x.ProfileId == CurrentProfileId.Value);
    }

    protected string? CurrentRoleName()
    {
        return CurrentAccess()?.Role?.RoleName;
    }

    protected bool HasRole(params string[] roles)
    {
        string? role = CurrentRoleName();
        return role != null && roles.Contains(role);
    }

    protected bool CanEditProducts()
    {
        return HasRole("Owner", "Accountant", "Manager");
    }

    protected bool CanManageFinance()
    {
        return HasRole("Owner", "Accountant");
    }

    protected bool CanManageProfile()
    {
        return HasRole("Owner");
    }

    protected bool EnsureProfileSelected()
    {
        if (CurrentProfileId != null)
            return true;

        if (CurrentUserId == null)
            return false;

        var firstAccess = Db.UserProfileAccesses
            .FirstOrDefault(x => x.UserId == CurrentUserId.Value);

        if (firstAccess == null)
            return false;

        HttpContext.Session.SetInt32("ProfileId", firstAccess.ProfileId);
        return true;
    }
}
