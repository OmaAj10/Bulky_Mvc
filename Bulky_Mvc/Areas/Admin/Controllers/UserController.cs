using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bulky_Mvc.Area.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = StaticDetails.Role_Admin)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RoleManagment(string userId)
    {
        var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

        RoleManagmentVM RoleVM = new RoleManagmentVM()
        {
            ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
            RoleList = _db.Roles.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name
            }),
            CompanyList = _db.Companies.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            })
        };

        RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
        
        return View(RoleVM);
    } 
    
    [HttpPost]
    public IActionResult RoleManagment(RoleManagmentVM roleManagmentVm)
    {
        var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVm.ApplicationUser.Id).RoleId;
        var oldRole = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

        if (!(roleManagmentVm.ApplicationUser.Role == oldRole))
        {
            // a role was updated
            ApplicationUser applicationUser =
                _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVm.ApplicationUser.Id);
            
            if (roleManagmentVm.ApplicationUser.Role == StaticDetails.Role_Company)
            {
                applicationUser.CompanyId = roleManagmentVm.ApplicationUser.CompanyId;
            }

            if (oldRole == StaticDetails.Role_Company)
            {
                applicationUser.CompanyId = null;
            }

            _db.SaveChanges();

            _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(applicationUser, roleManagmentVm.ApplicationUser.Role).GetAwaiter().GetResult();
        }

        return RedirectToAction("Index");
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

        var userRoles = _db.UserRoles.ToList();
        var roles = _db.Roles.ToList();

        foreach (var user in objUserList)
        {
            var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
            user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

            if (user.Company == null)
            {
                user.Company = new() { Name = "" };
            }
        }

        return Json(new { data = objUserList });
    }

    [HttpPost]
    public IActionResult LockUnlock([FromBody] string id)
    {
        var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

        if (objFromDb == null)
        {
            return Json(new { succes = false, message = "Error while Locking/Unlocking" });
        }

        if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
        {
            objFromDb.LockoutEnd = DateTime.Now;
        }
        else
        {
            objFromDb.LockoutEnd = DateTime.Now.AddYears(10);
        }

        _db.SaveChanges();
        return Json(new { succes = true, message = "Delete Successful" });
    }

    #endregion
}