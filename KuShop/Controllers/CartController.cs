using KuShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Dynamic;
using KuShop.ViewModels;

namespace KuShop.Controllers
{
    public class CartController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ตั้งชื่อว่า _db
        public readonly KuShopContext _db;
        //สร้าง Constructor สำหรับ Controller เพื่อใช้ obj ของ DBContext
        public CartController(KuShopContext db) { _db = db; }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddDtl(string pdid, int quantity = 1)
        {
            
            //ตรวจสอบ Login??
            if (HttpContext.Session.GetString("CusId") == null)
            {
                TempData["ErrorMessage"] = "Login ก่อนซื้อสินค้า";
                return RedirectToAction("Login", "Home"); //กลับไปหน้า LOGIN
            }
            //ตรวจสอบว่ามี pdid ส่งมา
            if (pdid == null)
            {
                TempData["ErrorMessage"] = "ต้องระบุรหัสสินค้า";
                return RedirectToAction("Index", "Home");
            }
            //ตรวจสอบว่ามีตะกร้า?? ถ้าไม่มีก็สร้างตะกร้า
            if (HttpContext.Session.GetString("CartId") == null)
            {
                return RedirectToAction("Add", new { pdid = pdid }); 
            }
            //ถ้ามีแล้วบันทึกข้อมูลสินค้าลงตะกร้า
            //หาข้อมูลสินค้า - ราคาขาย
            var pd = _db.Products.Find(pdid);
            string CartId = HttpContext.Session.GetString("CartId");

            // Find the cart detail for the given product ID
            var cdtl = _db.CartDtls.FirstOrDefault(cd => cd.CartId.Equals(CartId) && cd.PdId.Equals(pdid));

            if (cdtl == null)
            {
                // If the cart detail does not exist, create a new one
                cdtl = new CartDtl
                {
                    CartId = CartId,
                    PdId = pdid,
                    CdtlQty = quantity,
                    CdtlPrice = pd.PdPrice,
                    CdtlMoney = pd.PdPrice * quantity
                };
                _db.CartDtls.Add(cdtl);
            }
            else
            {
                // If the cart detail exists, update the quantity and total price
                cdtl.CdtlQty += quantity;
                cdtl.CdtlMoney = cdtl.CdtlPrice * cdtl.CdtlQty;
                _db.Entry(cdtl).State = EntityState.Modified;
            }

            _db.SaveChanges();

            //เมื่อ Detail เปลี่ยน Master ก็เปลี่ยน 
            //หายอดรวมของ Detail
            var CartMoney = _db.CartDtls.Where(a => a.CartId == CartId).Sum(a => a.CdtlMoney);
            var CartQty = _db.CartDtls.Where(a => a.CartId == CartId).Sum(a => a.CdtlQty);
            //Update db
            var cart = _db.Carts.Find(CartId);
            cart.CartQty = CartQty;
            cart.CartMoney = CartMoney;
            _db.SaveChanges();

            //Update Session เพื่อแสดงผล
            HttpContext.Session.SetString("CartQty", CartQty.ToString());
            HttpContext.Session.SetString("CartMoney", CartMoney.ToString());

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Add(string pdid)
        {
            //Gen CartId
            string theId;
            int rowCount = 0;
            int i = 0;
            string today;
            string CusId = HttpContext.Session.GetString("CusId");

            CultureInfo us = new CultureInfo("en-US");
            do
            {
                //สร้าง id จากปีเดิอนปัจจุบันต่อด้วยลำดับ รูปแบบ 000x
                i++;
                today = DateTime.Now.ToString("'CT'yyyyMMdd");
                theId = string.Concat(today, i.ToString("0000"));
                //ทำการตรวจสอบว่ามี CartId นี้แล้วหรือยัง
                var cart = from ct in _db.Carts
                           where ct.CartId.Equals(theId)
                           select ct;
                rowCount = cart.Count();
            } while (rowCount != 0);

            //ทำการบันทึกลง Table Cart
            try
            {
                //สร้าง Obj Cart และกำหนด Field ต่างๆ
                Cart obj = new Cart();
                obj.CartId = theId;
                obj.CusId = CusId;
                obj.CartDate = DateOnly.FromDateTime(DateTime.Now.Date);
                obj.CartQty = 0;
                obj.CartMoney = 0;
                //กำหนดสถานะทำงานเป็น Add และสั่งบันทึก
                _db.Entry(obj).State = EntityState.Added;
                _db.SaveChanges();

                //บันทึกลง Session Cart
                HttpContext.Session.SetString("CartId", theId);
                HttpContext.Session.SetString("CartQty", "0");
                HttpContext.Session.SetString("CartMoney", "0");

                return RedirectToAction("AddDtl", new { pdid = pdid });

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "การบันทึกผิดพลาด";
                return RedirectToAction("Index", "Home");
            }

        }

        public IActionResult Show(string cartid)
        {
            //Master
            //ตรวจสอบว่าเป็นตะกร้าของลูกค้าที่ใช้งานอยู่หรือไม่
            //ได้ข้อมูล Cart เป็นส่วน Master
            string cusid = HttpContext.Session.GetString("CusId");
            var cart = from ct in _db.Carts
                       where ct.CartId == cartid &&
                           ct.CusId == cusid
                       select ct;
            if (cart == null)
            {
                TempData["ErrorMessage"] = "ไม่พบตะกร้าที่ระบุ";
                return RedirectToAction("Index", "Home");
            }

            //Detail
            //เลือกข้อมูลของตะกร้า+สร้าง ViewModel CtdVM แสดงชื่อสินค้า
            var cartdtl = from ctd in _db.CartDtls
                          join p in _db.Products on ctd.PdId equals p.PdId
                              into join_ctd_p
                          from ctd_p in join_ctd_p.DefaultIfEmpty()
                          where ctd.CartId == cartid
                          select new CtdVM
                          {
                              CartId = ctd.CartId,
                              PdId = ctd.PdId,
                              PdName = ctd_p.PdName,
                              CdtlMoney = ctd.CdtlMoney,
                              CdtlPrice = ctd.CdtlPrice,
                              CdtlQty = ctd.CdtlQty
                          };

            //สร้าง Dynamic model เพื่อส่งข้อมูลให้ View เป็นสองตารางพร้อมกัน
            dynamic DyModel = new ExpandoObject();
            //ระบุส่วน Master รับข้อมูลจาก Obj cart
            DyModel.Master = cart;
            //ระบุส่วน Detail รับข้อมูลจาก Obj cartdtl
            DyModel.Detail = cartdtl;
            //ส่ง Dynamic Model ไปที่ View
            return View(DyModel);
        }

        public IActionResult Check()
        {
            // ตรวจสอบตะกร้า ของลูกค้าปัจจุบีน ที่ยังไม่ได้ทำการ CF - ถ้ามีค้างให้ใช้ CartId นั้น
            string cusid = HttpContext.Session.GetString("CusId");
            var cart = from ct in _db.Carts
                       where ct.CusId.Equals(cusid) && ct.CartCf != "Y"
                       select ct;
            int rowCount = cart.Count();
            // ถ้ามีตะกร้าค้าง
            if (rowCount != 0)
            {
                Cart obj = new Cart();
                foreach (var item in cart)
                {
                    obj = item;
                }
                //กำหนด Session ต่างๆของตะกร้า
                HttpContext.Session.SetString("CartId", obj.CartId);
                HttpContext.Session.SetString("CartQty", obj.CartQty.ToString());
                HttpContext.Session.SetString("CartMoney", obj.CartMoney.ToString());
            }
            return RedirectToAction("Shop", "Home");
        }

        public IActionResult Delete(string cartid)
        {
            //ตรวจสอบก่อนทำต่อ
            var cart = _db.Carts.Find(cartid);
            if (cart.CartCf == "Y")
            {
                //ถ้าตะกร้ายืนยันแล้ว
                return RedirectToAction("Shop", "Home");
            }

            //การลบตะกร้า คือลบทั้งเอกสาร ดั้งนั้นต้องลบตัวMaster และ Detail ด้วย
            //ลบส่วน Detail
            //เลือกรายการที่อยู่ในตะกร้า
            var detail = from ctd in _db.CartDtls
                         where ctd.CartId.Equals(cartid)
                         select ctd;
            //วน Loop ไล่ลบที่ละรายการ
            foreach (var item in detail)
            {
                _db.CartDtls.Remove(item);
            }
            _db.SaveChanges();

            //ลบส่วน Master
            //หาเอกสารที่ระบุ
            var master = _db.Carts.Find(cartid);
            if (master == null)
            {
                TempData["ErrorMessage"] = "ไม่พบตะกร้า";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            _db.Carts.Remove(master);
            _db.SaveChanges();

            //ลบตะกร้าแล้ว ลบ Session ด้วย
            HttpContext.Session.Remove("CartId");
            HttpContext.Session.Remove("CartQty");
            HttpContext.Session.Remove("CartMoney");

            TempData["SuccessMessage"] = "ยกเลิกคำสั่งซื้อแล้ว";
            return RedirectToAction("Shop", "Home");
        }

        public IActionResult DeleteDtl(string pdid, string cartid)
        {
            //ตรวจสอบก่อนทำต่อ
            var cfcart = _db.Carts.Find(cartid);
            if (cfcart.CartCf == "Y")
            {
                //ถ้าตะกร้ายืนยันแล้ว
                return RedirectToAction("Shop", "Home");
            }


            //ลบ Detail แต่ไม่ต้องวน Loop เพราะเลือกสินค้ามารายการเดียว
            var obj = _db.CartDtls.Find(cartid, pdid);
            if (obj == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูล";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            _db.CartDtls.Remove(obj);
            _db.SaveChanges();

            //เมื่อ Detail เปลี่ยน ทำการปรับยอดของ Master
            //เหมือนกับ AddDtl
            var cartmoney = _db.CartDtls.Where(a => a.CartId == cartid).Sum(a => a.CdtlMoney);
            var cartqty = _db.CartDtls.Where(a => a.CartId == cartid).Sum(a => a.CdtlQty);

            //ถ้าจำนวนสินค้าเป็น 0 ก็ลบตะกร้าทิ้ง
            if (cartqty == 0)
            {
                //ลบ Master
                var master = _db.Carts.Find(cartid);
                _db.Carts.Remove(master);
                _db.SaveChanges();

                //ลบตะกร้าแล้ว ลบSession ด้วย
                HttpContext.Session.Remove("CartId");
                HttpContext.Session.Remove("CartQty");
                HttpContext.Session.Remove("CartMoney");

                TempData["SuccessMessage"] = "ยกเลิกคำสั่งซื้อแล้ว";
                return RedirectToAction("Shop", "Home");
            }
            else
            {
                //Update Cart
                var cart = _db.Carts.Find(cartid);
                cart.CartQty = cartqty;
                cart.CartMoney = cartmoney;
                _db.SaveChanges();

                //Update Session
                HttpContext.Session.SetString("CartMoney", cartmoney.ToString());
                HttpContext.Session.SetString("CartQty", cartqty.ToString());

                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
        }

        public IActionResult Confirm(string cartid)
        {
            //ตรวจสอบก่อนทำต่อ
            var cart = _db.Carts.Find(cartid);
            if (cart.CartCf == "Y")
            {
                //ถ้าตะกร้ายืนยันแล้ว
                return RedirectToAction("Shop", "Home");
            }

            //หาสินค้าที่อยู่ใน Detail เพื่อไปตัด Stock สินค้า
            var cartdtl = from ctd in _db.CartDtls
                          where ctd.CartId.Equals(cartid)
                          select ctd;
            int rowCount = cartdtl.Count();
            if (rowCount == 0)
            {
                TempData["ErrorMessage"] = "การยืนยันผิดพลาด";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            //วน Loop แต่ละสินค้า
            foreach (var detail in cartdtl)
            {
                //โดยปกติ ASP.NET Core จะไม่ให้ Connect DBContext ซ้อนกันต้องปิด Connection ก่อน
                //แก้ไขโดย Set Connection String ใน appsetting.json
                //เพิ่ม MultipleActiveResultSets=True ต่อท้าย
                Product pd = _db.Products.Find(detail.PdId);
                //Update Stock และวันที่ขายล่าสุด
                pd.PdStk = pd.PdStk - detail.CdtlQty;
                pd.PdLastSale = DateOnly.FromDateTime(DateTime.Now.Date);
                _db.Entry(pd).State = EntityState.Modified;
            }
            _db.SaveChanges();

            //Update ตะกร้าว่าทำการยืนยันแล้ว
            var master = _db.Carts.Find(cartid);
            master.CartCf = "Y";
            _db.Entry(master).State = EntityState.Modified;
            _db.SaveChanges();

            //ลบ Session ตะกร้า
            HttpContext.Session.Remove("CartId");
            HttpContext.Session.Remove("CartQty");
            HttpContext.Session.Remove("CartMoney");

            TempData["SuccessMessage"] = "ยืนยันคำสั่งซื้อแล้ว";
            return RedirectToAction("Shop", "Home");
        }

        public IActionResult List(string cusid)
        {
            //เลือกตะกร้าทั้งหมด ที่เป็นของลูกค้าที่ระบุ
            var cart = from c in _db.Carts
                       where c.CusId.Equals(cusid)
                       orderby c.CartId descending
                       select c;
            return View(cart);
        }
    }
}
