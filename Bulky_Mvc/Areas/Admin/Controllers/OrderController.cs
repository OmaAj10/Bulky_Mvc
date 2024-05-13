using Microsoft.AspNetCore.Mvc;

namespace Bulky_Mvc.Area.Admin.Controllers;

public class OrderController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}