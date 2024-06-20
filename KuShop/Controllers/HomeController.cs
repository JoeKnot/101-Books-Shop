using KuShop.Models;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace KuShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly KuShopContext _db;

        public HomeController(KuShopContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Shop");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string userName,string userPass)
        {
            var cus = from c in _db.Customers
                      where c.CusLogin.Equals(userName)
                      && c.CusPass.Equals(userPass)
                      select c;

            if(cus.ToList().Count()==0)
            {
                TempData["ErrorMessage"] = "ไม่พบผู้ใช้";
                return RedirectToAction("Index");
            }

            string CusId;
            string CusName;

            foreach(var item in cus)
            {
                CusId = item.CusId;
                CusName = item.CusName;

                HttpContext.Session.SetString("CusId", CusId);
                HttpContext.Session.SetString("CusName", CusName);

                var theRecord = _db.Customers.Find(CusId);
                theRecord.LastLogin = DateOnly.FromDateTime( DateTime.Now);

                _db.Entry(theRecord).State = EntityState.Modified;
            }

            _db.SaveChanges();
            
            return RedirectToAction("Check","Cart");
            
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Shop(string? stext)
        {
            // Use LINQ to Entities to filter products based on the search text
            var pdvm = from p in _db.Products
                       join pt in _db.ProductTypes on p.PdtId equals pt.PdtId into join_p_pt
                       from p_pt in join_p_pt.DefaultIfEmpty()
                       join b in _db.Categories on p.CategoryID equals b.CategoryID into join_p_b
                       from p_b in join_p_b.DefaultIfEmpty()
                       select new PdVM
                       {
                           PdId = p.PdId,
                           PdName = p.PdName,
                           PdtName = p_pt.PdtName,
                           CategoryName = p_b.CategoryName,
                           PdPrice = p.PdPrice,
                           PdCost = p.PdCost,
                           PdStk = p.PdStk
                       };

            // If stext is not null, filter the products
            if (!string.IsNullOrEmpty(stext))
            {
                pdvm = pdvm.Where(p => p.PdName.Contains(stext) || p.CategoryName.Contains(stext) || p.PdtName.Contains(stext));
            }

            // Pass the search text back to the view so it can be used in the search input
            ViewBag.stext = stext;

            return View(pdvm);
        }

        public IActionResult Login()
        {
            return View();
        }

    }
}