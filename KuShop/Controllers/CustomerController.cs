using KuShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace KuShop.Controllers
{
    public class CustomerController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;
        

        //สร้าง Constructor สำหรับตัว Controller ใช้งาน Obj ของ DBContext
        // สร้างตัวแปร _db สำหรับการเข้าถึงฐานข้อมูล KuShop
        //รับ KuShopContext instance ผ่าน constructor
        //เก็บ KuShopContext instance ไว้ในตัวแปร _db เพื่อใช้งานใน controller
        public CustomerController(KuShopContext db)
        {
            //สร้างตัวแปร _db สำหรับการเข้าถึงฐานข้อมูล KuShop
            _db = db;
            
        }

        //สร้าง Action Method เพื่อทำงานแสดงข้อมูล Customer
        public IActionResult Show(string id) 
        { 
            //ตรวจสอบว่ามี id ส่งมาหรือไม่
            if(id==null)
            {
                TempData["ErrorMessage"] = "ต้องระบุ id";
                return RedirectToAction("Index");
            }
            // หาข้อมูลของ Customer.CusId จาก id ที่ส่งมา
            var obj = _db.Customers.Find(id);
            if(obj==null)
            {
                TempData["ErrorMessage"] = "ไม่พบ id ที่ระบุ";
                return RedirectToAction("Index");
            }
            //ตั้งชื่อ File img ของ Customer เป็น <รหัสผู้ใช้>.jpg
            var fileName = id.ToString()+".jpg";
            // กำหนด Path - Directory ที่เก็บรูป -> imgcus 
            // แล้วทำมาต่อ Path อ้างอิ่งกับตำแหน่งที่ทำงานปัจจุบัน
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            // เอา Path และ ชื่อ File มาต่อกัน
            var filePath = Path.Combine(imgPath, fileName);
            
            //ตรวจสอบว่ามี File อยู่ตาม Path ที่กำหนดหรือไม่
            //ถ้ามีก็ส่ง Path ไปให้ View ผ่าน ViewBag
            //ถ้าไม่มี ก็ให้เรียกรูปภาพ Default ที่สร้างไว้
            if (System.IO.File.Exists(filePath))
                ViewBag.ImgFile = "/imgcus/" + id + ".jpg";
            else
                ViewBag.ImgFile = "/image/login.png";

            //ถ้าหา id เจอส่ง obj ที่ได้จาก Query ไปให้ View
            return View(obj);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ImgUpload(IFormFile imgfiles,string theid)
        { 
            // กำหนดตัวแปรชื่อ File , Extension ของ File
            // รวมกันเป็นชื่อ File ที่ต้องการ Save
            var FileName = theid;
            var FileExtension = Path.GetExtension(imgfiles.FileName);
            var SaveFileName = FileName + FileExtension;
            // กำหนดตำแหน่งที่จะ Save File
            var SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            // รวมชื่อและตำแหน่งที่จะ Save File
            var SaveFilePath = Path.Combine(SavePath, SaveFileName);
            //สั่งให้อ่าน File มาเป็น Stream และ Save ลงตำแหน่งที่กำหนด
            using (FileStream fs = System.IO.File.Create(SaveFilePath))
            {
                imgfiles.CopyTo(fs);
                fs.Flush();
            }
            // ย้ายไปทำงานที่ Action Show โดยกำหนดตัวแปร id จากตัวแปร theid
            return RedirectToAction("Show",new { id = theid });
        }

        public IActionResult ImgDelete(string id)
        {
            var DeleteFileName = id + ".jpg";
            var DeletePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            var DeleteFilePath = Path.Combine(DeletePath, DeleteFileName);
            if(System.IO.File.Exists(DeleteFilePath))
            {
                System.IO.File.Delete(DeleteFilePath);
            }
            else
            {
                TempData["ErrorMessage"] = "ไม่มีรูปที่ระบุ";
            }
            return RedirectToAction("Show",new { id = id });    
        }
        // GET: /Customer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Generate Customer Code
                    string customerCode = GenerateCustomerCode();

                    // Set Customer Code to the object
                    obj.CusId = customerCode;

                    _db.Customers.Add(obj);  //ส่งคำสั่ง Add ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Login", "Home"); //ย้ายหน้าทำงานไปที่ Action Index
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


        // Generate unique customer code
        private string GenerateCustomerCode()
        {
            // Logic to generate customer code here
            // Example logic to generate a random code
            Random rand = new Random();
            return "c" + rand.Next(100000, 999999).ToString();
        }
    }
}
