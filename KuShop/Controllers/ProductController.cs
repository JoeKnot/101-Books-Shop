using KuShop.Models;
using Microsoft.AspNetCore.Mvc;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KuShop.Controllers
{
    public class ProductController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ตั้งชื่อว่า _db
        public readonly KuShopContext _db;
        //สร้าง Constructor สำหรับ Controller เพื่อใช้ obj ของ DBContext
        public ProductController(KuShopContext db) { _db = db; }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["ErrorMessage"] = "ไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }


            //var pd = from p in _db.Products select p;

            //สร้าง Object รับค่าเพื่อส่งไปให้ View
            //โดยการ Join กันระหว่างตาราง Products กับ ProductTypes ผ่าน PdtId
            //      Join กันระหว่างตาราง Products กับ Brands ผ่าน BrandId
            //การ Query ผ่าน LINQ จะต้องสร้าง Object มารับค่าจาก Class(Model) ที่เชื่อมต่อกับ Table ใน DB
            //ดังนั้นจะมีการประกาศตัวแปรมากมาย
            // .DefaultIfEmpty() เป็นการกำหนดว่าให้ดึงค่าจากตารางหลักถ้าไม่มีลัพธการ Join ***มันคือ LEFT JOIN
            // เลือก Field ที่ต้องนำมาแสดงเพื่อใส่ค่าลงใน Obj pdvm ที่สร้างจาก Class PdVM 
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

            //ถ้าค่าที่อ่านได้เป็น Null ก็ Return เรียกFunction NotFound()
            if (pdvm == null) return NotFound();

            //ถ้าพบ ส่ง Obj pd ที่ได้ให้ View ไปแสดง
            return View(pdvm);
        }

        //สร้าง ActionMethod ใหม่เพื่อรองรับการส่งข้อมูลแบบ Post
        //คัดลอกมาจาก Index() เริ่มต้น (แบบ GET) แล้วแก้ไข
        [HttpPost] //ระบุว่าเป็นการทำงานแบบ POST
        [ValidateAntiForgeryToken] //ป้องกันการโจมตี Cross-Site Request Forgery
        public IActionResult Index(string? stext) //ค่าที่รับมาจาก View (Form)
        {
            if (stext == null) // ถ้าไม่มีการส่งค่าอะไรมา
            {
                return RedirectToAction("Index"); //ให้ย้ายไปทำ ActionMethod Index (แบบปกติ GET)
            }

            var pdvm = from p in _db.Products

                       join pt in _db.ProductTypes on p.PdtId equals pt.PdtId into join_p_pt
                       from p_pt in join_p_pt.DefaultIfEmpty()

                       join b in _db.Categories on p.CategoryID equals b.CategoryID into join_p_b
                       from p_b in join_p_b.DefaultIfEmpty()
                           //ข้อแม้ในการค้นหาข้อมูล ใช้method Contains()
                       where p.PdName.Contains(stext) ||
                            p_b.CategoryName.Contains(stext) ||
                            p_pt.PdtName.Contains(stext)

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
            if (pdvm == null) return NotFound();

            //ส่งข้อมูลให้ View ผ่าน ViewBag สร้าง ViewBag stext ให้ค่าเท่ากับตัวแปร stext
            ViewBag.stext = stext;

            return View(pdvm);
        }

        //การเรียก Link เป็นการทำงานแบบ GET
        //เป็นการเรียกหน้า ไม่มีการคำนวณ ทำการ return ไปที่ View ได้เลย
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["ErrorMessage"] = "ไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ตัวแปร ViewData
            //ชื่อตัวแปรตั้งเองได้
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName");
            ViewData["Category"] = new SelectList(_db.Categories, "CategoryID", "CategoryName");
            return View();
        }
        [HttpPost] //ระบุว่ารับค่ามาแบบ POST
        [ValidateAntiForgeryToken] //ป้องกันการโจมตี แบบ Cross_site Request Forgery
        //ค่าที่ส่งมาจาก Form จะเป็น Object ของ Model ฝั่งController ก็ต้องรับเป็น Object ด้วย
        public IActionResult Create(Product obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Products.Add(obj);  //ส่งคำสั่ง Add ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Index"); //ย้ายหน้าทำงานไปที่ Action Index
                }
            }
            catch (Exception ex)
            {
                //ถ้าไม่ Valid ให้สร้าง Error Message และส่ง ให้ View เพื่อแสดงผล
                ViewBag.ErrorMessage = ex.Message;
                return View(obj);
            }
            //ถ้าไม่ Valid ให้สร้าง Error Message และส่ง ให้ View เพื่อแสดงผล
            ViewBag.ErrorMessage = "การบันทึกผิดพลาด";
            return View(obj);
        }

        public IActionResult Edit(string id)
        {
            //ตรวจสอบว่ามีการส่ง id หรือไม่
            if (id == null)
            {
                ViewBag.ErrorMessage = "ระบุ id";
                return RedirectToAction("Index");
            }
            //ค้นหา Record ของ Product.pdId จากค่า id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                ViewBag.ErrorMessage = "ไม่พบข้อมูล";
                return RedirectToAction("Index");
            }
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ตัวแปร ViewData
            //ระบุใน SelectList ให้แสดงข้อมูลของ Obj ที่ตรงกับ id 
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName", obj.PdId);
            ViewData["Category"] = new SelectList(_db.Categories, "CategoryID", "CategoryName", obj.CategoryID);
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imgpd");
            var filePath = Path.Combine(imgPath, id + ".jpg");
            ViewBag.ImgFile = System.IO.File.Exists(filePath) ? "/imgpd/" + id + ".jpg" : "/image/login.png";
            return View(obj);
        }

        [HttpPost] //ระบุว่ารับค่ามาแบบ POST
        [ValidateAntiForgeryToken] //ป้องกันการโจมตี แบบ Cross_site Request Forgery
        //ค่าที่ส่งมาจาก Form จะเป็น Object ของ Model ฝั่งController ก็ต้องรับเป็น Object ด้วย
        public IActionResult Edit(Product obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Products.Update(obj);  //ส่งคำสั่ง Update ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Index"); //ย้ายหน้าทำงานไปที่ Action Index
                }
            }
            catch (Exception ex)
            {
                //ถ้าไม่ Valid ให้สร้าง Error Message และส่ง ให้ View เพื่อแสดงผล
                ViewBag.ErrorMessage = ex.Message;
                return View(obj);
            }
            //ถ้าไม่ Valid ให้สร้าง Error Message และส่ง ให้ View เพื่อแสดงผล
            ViewBag.ErrorMessage = "การแก้ไขผิดพลาด";
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ตัวแปร ViewData
            //ระบุใน SelectList ให้แสดงข้อมูลของ Obj ที่ตรงกับ id 
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName", obj.PdId);
            ViewData["Category"] = new SelectList(_db.Categories, "CategoryID", "CategoryName", obj.CategoryID);
            return View(obj);
        }

        public IActionResult Delete(String id)
        {
            //ตรวจสอบว่ามีการส่ง id หรือไม่
            if (id == null)
            {
                ViewBag.ErrorMessage = "ระบุ id";
                return RedirectToAction("Index");
            }
            //ค้นหา Record ของ Product.pdId จากค่า id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                ViewBag.ErrorMessage = "ไม่พบข้อมูล";
                return RedirectToAction("Index");
            }
            
            ViewBag.categoryName = _db.Categories.FirstOrDefault(br => br.CategoryID == obj.CategoryID).CategoryName;
            return View(obj);

        }

        [HttpPost] //ระบุว่ารับค่ามาแบบ POST
        [ValidateAntiForgeryToken] //ป้องกันการโจมตี แบบ Cross_site Request Forgery
        //การตั้งชื่อ Method ถ้าชื่อเหมือนกัน Parameter ต้องไม่เหมือนกัน
        //กรณีนี้ Parameter เป็น String ทั้งคู่(GET,POST) จึงตั้งชื่อ Method เป็น DeletePost
        //ชื่อ Parameter ที่รับต้องเหมือนกันฝั่งที่ส่งมาจาก View ด้วยจึงใช้ Pdid
        public IActionResult DeletePost(string Pdid)
        {
            try
            {
                //หาส Record ของ Product.Pdid จาก Pdid ที่ส่งมา
                var obj = _db.Products.Find(Pdid);
                if (obj == null)
                {
                    ViewBag.ErrorMessage = "ไม่พบข้อมูล";
                    return RedirectToAction("Index");
                }
                _db.Products.Remove(obj); // สั่ง Remove
                _db.SaveChanges(); //Execute คำสั่ง
                return RedirectToAction("Index"); //ย้ายไป Action index

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult Show(string id)
        {
            //ตรวจสอบว่ามี id ส่งมาหรือไม่
            if (id == null)
            {
                TempData["ErrorMessage"] = "ต้องระบุ id";
                return RedirectToAction("Index");
            }
            // หาข้อมูลของ Customer.CusId จาก id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                TempData["ErrorMessage"] = "ไม่พบ id ที่ระบุ";
                return RedirectToAction("Index");
            }
            //ตั้งชื่อ File img ของ Customer เป็น <รหัสผู้ใช้>.jpg
            var fileName = id.ToString() + ".jpg";
            // กำหนด Path - Directory ที่เก็บรูป -> imgcus 
            // แล้วทำมาต่อ Path อ้างอิ่งกับตำแหน่งที่ทำงานปัจจุบัน
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgpd");
            // เอา Path และ ชื่อ File มาต่อกัน
            var filePath = Path.Combine(imgPath, fileName);

            //ตรวจสอบว่ามี File อยู่ตาม Path ที่กำหนดหรือไม่
            //ถ้ามีก็ส่ง Path ไปให้ View ผ่าน ViewBag
            //ถ้าไม่มี ก็ให้เรียกรูปภาพ Default ที่สร้างไว้
            if (System.IO.File.Exists(filePath))
                ViewBag.ImgFile = "/imgpd/" + id + ".jpg";
            else
                ViewBag.ImgFile = "/image/login.png";

            //ถ้าหา id เจอส่ง obj ที่ได้จาก Query ไปให้ View
            return View(obj);
        }
    }
}
