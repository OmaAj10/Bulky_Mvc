using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RoleManagment(string userId)
    {
        RoleManagmentVM RoleVM = new RoleManagmentVM()
        {
            ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
            RoleList = _roleManager.Roles.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name
            }),
            CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            })
        };

        RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId))
            .GetAwaiter()
            .GetResult()
            .FirstOrDefault();
        
        return View(RoleVM);
    } 
    
    [HttpPost]
    public IActionResult RoleManagment(RoleManagmentVM roleManagmentVm)
    {
        var oldRole= _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVm.ApplicationUser.Id))
            .GetAwaiter()
            .GetResult()
            .FirstOrDefault();
        
        ApplicationUser applicationUser =
            _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVm.ApplicationUser.Id);


        if (!(roleManagmentVm.ApplicationUser.Role == oldRole))
        {
            // a role was updated
        
            if (roleManagmentVm.ApplicationUser.Role == StaticDetails.Role_Company)
            {
                applicationUser.CompanyId = roleManagmentVm.ApplicationUser.CompanyId;
            }

            if (oldRole == StaticDetails.Role_Company)
            {
                applicationUser.CompanyId = null;
            }
            
            _unitOfWork.ApplicationUser.Update(applicationUser);
            _unitOfWork.Save();

            _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(applicationUser, roleManagmentVm.ApplicationUser.Role).GetAwaiter().GetResult();
        }
        else
        {
            if (oldRole == StaticDetails.Role_Company && applicationUser.CompanyId != roleManagmentVm.ApplicationUser.CompanyId)
            {
                applicationUser.CompanyId = roleManagmentVm.ApplicationUser.CompanyId;
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
            }
        }
        

        return RedirectToAction("Index");
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();
        
        foreach (var user in objUserList)
        {
            user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

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
        var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);

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
        
        _unitOfWork.ApplicationUser.Update(objFromDb);
        _unitOfWork.Save();
        return Json(new { succes = true, message = "Delete Successful" });
    }

    #endregion
}