using System;
using System.Collections.Generic;

namespace KuShop.Models;

public partial class Work
{
    public string StfId { get; set; } = null!;

    public DateOnly? WorkDate { get; set; }

    public string? WorkIn { get; set; }

    public string? WorkOut { get; set; }
}
