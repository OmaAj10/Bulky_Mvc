using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulky_Mvc.Area.Admin.Controllers;

[Area("Admin")]
// [Authorize(Roles = StaticDetails.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _UnitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        _UnitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var objCompanyList = _UnitOfWork.Company.GetAll().ToList();

        return View(objCompanyList);
    }

    //Update and insert
    public IActionResult Upsert(int? id)
    {
        if (id == null || id == 0)
        {
            return View(new Company());
        }
        else
        {
            Company companyObj = _UnitOfWork.Company.Get(u => u.Id == id);
            return View(companyObj);
        }
    }

    [HttpPost]
    public IActionResult Upsert(Company companyObj)
    {
        if (ModelState.IsValid)
        {
            if (companyObj.Id == 0)
            {
                _UnitOfWork.Company.Add(companyObj);
            }
            else
            {
                _UnitOfWork.Company.Update(companyObj);
            }

            _UnitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            return View(companyObj);
        }
    }

    #region 

    [HttpGet]
    public IActionResult GetAll()
    {
        var objCompanyList = _UnitOfWork.Company.GetAll().ToList();
        return Json(new { data = objCompanyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var companyToBeDeleted = _UnitOfWork.Company.Get(u => u.Id == id);

        if (companyToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        
        _UnitOfWork.Company.Remove(companyToBeDeleted);
        _UnitOfWork.Save();

        return Json(new { succes = true, message = "Delete Successful" });
    }

    #endregion
}