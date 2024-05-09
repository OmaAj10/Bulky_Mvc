using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky_Mvc.Area.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IWebHostEnvironment _WebHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _UnitOfWork = unitOfWork;
        _WebHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        var objProdcutList = _UnitOfWork.Product.GetAll(includeProperties: "Category").ToList();

        return View(objProdcutList);
    }

    //Updaten and insert
    public IActionResult Upsert(int? id)
    {
        //Projections in EF Core
        //Pick only som columns from Category and convert that to new object 
        ProductVM productVM = new()
        {
            CategoryList = _UnitOfWork.Category.GetAll().Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            }),
            Product = new Product()
        };

        if (id == null || id == 0)
        {
            //Create
            return View(productVM);
        }
        else
        {
            //Update
            productVM.Product = _UnitOfWork.Product.Get(u => u.Id == id);
            return View(productVM);
        }
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _WebHostEnvironment.WebRootPath;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images/product");

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    //delete the old image
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = @"/images/product/" + fileName;
            }

            if (productVM.Product.Id == 0)
            {
                _UnitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _UnitOfWork.Product.Update(productVM.Product);
            }

            _UnitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            productVM.CategoryList = _UnitOfWork.Category.GetAll().Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });

            return View(productVM);
        }
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