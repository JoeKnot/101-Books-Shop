using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KuShop.Models;

public partial class Customer
{
    [Key]
    [Required(ErrorMessage ="ต้องระบุรหัส")]
    [Display(Name ="รหัส")]
    public string CusId { get; set; } = null!;

    [Required(ErrorMessage = "ต้องระบุชื่อ")]
    [Display(Name = "ชื่อ นามสกุล")]
    public string CusName { get; set; } = null!;
    
    [Required(ErrorMessage = "ต้องระบุ Login")]
    [Display(Name = "Login Name")]
    public string CusLogin { get; set; } = null!;
    
    [Required(ErrorMessage = "ต้องระบุรหัสผ่าน")]
    [Display(Name = "รหัสผ่าน")]
    public string CusPass { get; set; } = null!;
    
    [Display(Name = "Email")]
    public string CusEmail { get; set; } = null!;

    [Display(Name = "ที่อยู่")]
    public string? CusAdd { get; set; }

    [Display(Name = "วันที่สมัคร")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "วันใช้งานล่าสุด")]
    public DateOnly? LastLogin { get; set; }
    
}
