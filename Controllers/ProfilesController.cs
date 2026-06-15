using FopAccounting.Data;
using FopAccounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FopAccounting.Controllers;

public class ProfilesController : BaseController
{
    public ProfilesController(AppDbContext db) : base(db) { }

    public IActionResult Index()
    {
        if (!IsAuthorized()) return RedirectToLogin();

        FillLayoutData();

        var accesses = Db.UserProfileAccesses
            .Include(x => x.Role)
            .Include(x => x.Profile)
            .Where(x => x.UserId == CurrentUserId!.Value)
            .ToList();

        ViewBag.Accesses = accesses;

        var profiles = accesses
            .Where(x => x.Profile != null)
            .Select(x => x.Profile!)
            .ToList();

        return View(profiles);
    }

    public IActionResult Select(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        bool hasAccess = Db.UserProfileAccesses
            .Any(x => x.UserId == CurrentUserId!.Value && x.ProfileId == id);

        if (!hasAccess)
            return RedirectToAction("Index");

        HttpContext.Session.SetInt32("ProfileId", id);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Create()
    {
        if (!IsAuthorized()) return RedirectToLogin();

        FillLayoutData();

        return View(new BusinessProfile());
    }

    [HttpPost]
    public IActionResult Create(BusinessProfile profile)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        profile.Id = 0;
        profile.OwnerId = CurrentUserId!.Value;
        profile.CreatedAt = DateTime.Now;

        Db.BusinessProfiles.Add(profile);
        Db.SaveChanges();

        var ownerRole = GetOrCreateRole("Owner");

        if (ownerRole == null)
        {
            ownerRole = new Role
            {
                RoleName = "Owner"
            };

            Db.Roles.Add(ownerRole);
            Db.SaveChanges();
        }

        Db.UserProfileAccesses.Add(new UserProfileAccess
        {
            UserId = CurrentUserId.Value,
            ProfileId = profile.Id,
            RoleId = ownerRole.Id
        });

        Db.SaveChanges();

        HttpContext.Session.SetInt32("ProfileId", profile.Id);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Edit(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        bool hasAccess = Db.UserProfileAccesses
            .Any(x => x.UserId == CurrentUserId!.Value && x.ProfileId == id);

        if (!hasAccess)
            return RedirectToAction("Index");

        HttpContext.Session.SetInt32("ProfileId", id);

        if (!CanManageProfile())
            return RedirectToAction("Index");

        FillLayoutData();

        var profile = Db.BusinessProfiles
            .FirstOrDefault(x => x.Id == id);

        if (profile == null)
            return RedirectToAction("Index");

        return View(profile);
    }

    [HttpPost]
    public IActionResult Edit(BusinessProfile model)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        bool hasAccess = Db.UserProfileAccesses
            .Any(x => x.UserId == CurrentUserId!.Value && x.ProfileId == model.Id);

        if (!hasAccess)
            return RedirectToAction("Index");

        HttpContext.Session.SetInt32("ProfileId", model.Id);

        if (!CanManageProfile())
            return RedirectToAction("Index");

        var profile = Db.BusinessProfiles
            .FirstOrDefault(x => x.Id == model.Id);

        if (profile == null)
            return RedirectToAction("Index");

        profile.ProfileName = model.ProfileName;
        profile.BusinessType = model.BusinessType;
        profile.TaxNumber = model.TaxNumber;
        profile.Phone = model.Phone;
        profile.Address = model.Address;
        profile.Description = model.Description;

        Db.SaveChanges();

        return RedirectToAction("Index");
    }

    public IActionResult Accesses()
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index");
        if (!CanManageProfile()) return RedirectToAction("Index");

        FillLayoutData();

        ViewBag.Users = Db.Users
            .OrderBy(x => x.FullName)
            .ToList();

        ViewBag.Accesses = Db.UserProfileAccesses
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.ProfileId == CurrentProfileId!.Value)
            .ToList();

        return View();
    }

    [HttpPost]
    public IActionResult AddAccess(int userId, string role)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index");
        if (!CanManageProfile()) return RedirectToAction("Index");

        var user = Db.Users.FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return RedirectToAction("Accesses");

        var dbRole = GetOrCreateRole(role);

        var access = Db.UserProfileAccesses
            .FirstOrDefault(x =>
                x.ProfileId == CurrentProfileId!.Value &&
                x.UserId == userId);

        if (access == null)
        {
            Db.UserProfileAccesses.Add(new UserProfileAccess
            {
                UserId = userId,
                ProfileId = CurrentProfileId.Value,
                RoleId = dbRole.Id
            });
        }
        else
        {
            access.RoleId = dbRole.Id;
        }

        Db.SaveChanges();

        return RedirectToAction("Accesses");
    }

    public IActionResult RemoveAccess(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();
        if (!EnsureProfileSelected()) return RedirectToAction("Index");
        if (!CanManageProfile()) return RedirectToAction("Index");

        var access = Db.UserProfileAccesses
            .FirstOrDefault(x =>
                x.Id == id &&
                x.ProfileId == CurrentProfileId!.Value);

        if (access != null && access.UserId != CurrentUserId!.Value)
        {
            Db.UserProfileAccesses.Remove(access);
            Db.SaveChanges();
        }

        return RedirectToAction("Accesses");
    }

    private Role GetOrCreateRole(string roleName)
    {
        roleName = (roleName ?? string.Empty).Trim();

        var role = Db.Roles.FirstOrDefault(x => x.RoleName == roleName);

        if (role == null)
        {
            role = new Role { RoleName = roleName };
            Db.Roles.Add(role);
            Db.SaveChanges();
        }

        return role;
    }
}