using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KuShop.Models;

public partial class Product
{
    [Key]
    [Required(ErrorMessage ="ต้องระบุรหัสสินค้า")]
    [Display(Name ="รหัสสินค้า")]
    public string PdId { get; set; } = null!;
    [Required(ErrorMessage ="ต้องระบุชื่อสินค้า")]
    [StringLength(100)]
    [Display(Name ="ชื่อสินค้า")]
    public string PdName { get; set; } = null!;
    [Display(Name ="ประเภท")]
    public byte? PdtId { get; set; }
    [Display(Name = "หมวดหมู่")]
    public byte? CategoryID { get; set; }
    [Display(Name = "ราคา")]
    public double? PdPrice { get; set; }
    [Display(Name = "ต้นทุน")]
    public double? PdCost { get; set; }
    [Display(Name = "จำนวน")]
    public double? PdStk { get; set; }
    [Display(Name = "วันที่ขายล่าสุด")]
    public DateOnly? PdLastBuy { get; set; }
    [Display(Name = "วันที่ซื้อล่าสุด")]
    public DateOnly? PdLastSale { get; set; }
    [Display(Name = "คำอธิบายสินค้า")]
    public string PdDs { get; set; } = null!;

    

}
