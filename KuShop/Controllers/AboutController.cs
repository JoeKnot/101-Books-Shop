using Microsoft.AspNetCore.Mvc;

namespace KuShop.Controllers
{
    public class AboutController : Controller
    {
        //Action Method -> Index()
        //Method แรกที่สร้างให้คือ Index()
        public IActionResult Index()
        {
            return View();
        }
    }
}
