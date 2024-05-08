using System.IO.Compression;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky_Mvc.Area.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _UnitOfWork;

    public ProductController(IUnitOfWork unitOfWork)
    {
        _UnitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var objProdcutList = _UnitOfWork.Product.GetAll().ToList();

        return View(objProdcutList);
    }

    public IActionResult Create()
    {
        //Projections in EF Core
        //Pick only som columns from Category and convert that to new object 
        IEnumerable<SelectListItem> CategoryList = _UnitOfWork.Category.GetAll().Select(s => new SelectListItem
        {
            Text = s.Name,
            Value = s.Id.ToString()
        });

        ViewBag.CategoryList = CategoryList;
        // ViewData["CategoryList"] = CategoryList;

        return View();
    }

    [HttpPost]
    public IActionResult Create(Product obj)
    {
        if (ModelState.IsValid)
        {
            _UnitOfWork.Product.Add(obj);
            _UnitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        Product? productFromDb = _UnitOfWork.Product.Get(u => u.Id == id);

        if (productFromDb == null)
        {
            return NotFound();
        }

        return View(productFromDb);
    }

    [HttpPost]
    public IActionResult Edit(Product obj)
    {
        if (ModelState.IsValid)
        {
            _UnitOfWork.Product.Update(obj);
            _UnitOfWork.Save();
            TempData["success"] = "Product updated successfully";
            return RedirectToAction("Index");
        }

        return View();
    }

    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        Product? productFromDb = _UnitOfWork.Product.Get(u => u.Id == id);

        if (productFromDb == null)
        {
            return NotFound();
        }

        return View(productFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePost(int? id)
    {
        Product? obj = _UnitOfWork.Product.Get(d => d.Id == id);

        if (obj == null)
        {
            return NotFound();
        }

        _UnitOfWork.Product.Remove(obj);
        _UnitOfWork.Save();
        TempData["success"] = "Product deleted successfully";
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
}