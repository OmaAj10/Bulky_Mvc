using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky_Mvc.Area.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = StaticDetails.Role_Admin)]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        var objProdcutList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

        return View(objProdcutList);
    }

    //Update and insert
    public IActionResult Upsert(int? id)
    {
        //Projections in EF Core
        //Pick only som columns from Category and convert that to new object 
        ProductVM productVM = new()
        {
            CategoryList = _unitOfWork.Category.GetAll().Select(s => new SelectListItem
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
            productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
            return View(productVM);
        }
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
    {
        if (ModelState.IsValid)
        {
            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }

            _unitOfWork.Save();

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (IFormFile file in files)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var productPath = @"images/products/product-" + productVM.Product.Id;
                    var finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);

                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    ProductImage productImage = new()
                    {
                        ImageUrl = @"\" + productPath + @"\" + fileName,
                        ProductId = productVM.Product.Id,
                    };

                    if (productVM.Product.ProductImages == null)
                        productVM.Product.ProductImages = new List<ProductImage>();

                    productVM.Product.ProductImages.Add(productImage);
                }

                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
            }

            TempData["success"] = "Product created/updated successfully";
            return RedirectToAction("Index");
        }
        else
        {
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(productVM);
        }
    }

    public IActionResult DeleteImage(int imageId)
    {
        var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
        var productId = imageToBeDeleted.ProductId;
        
        if (imageToBeDeleted != null)
        {
            if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('/'));
                
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            
            _unitOfWork.ProductImage.Remove(imageToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Deleted successfully";
        }

        return RedirectToAction(nameof(Upsert), new { id = productId });
    }

    #region Api CAlLS

    [HttpGet]
    public IActionResult GetAll()
    {
        var objProdcutList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json(new { data = objProdcutList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);

        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        
        var productPath = @"images/products/product-" + id;
        var finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

        if (Directory.Exists(finalPath))
        {
            string[] filePaths = Directory.GetFiles(finalPath);

            foreach (var filePath in filePaths)
            {
                System.IO.File.Delete(filePath);
            }
            
            Directory.Delete(finalPath);
        }
        
        _unitOfWork.Product.Remove(productToBeDeleted);
        _unitOfWork.Save();

        return Json(new { succes = true, message = "Delete Successful" });
    }

    #endregion
}