namespace KuShop.ViewModels
{
    public class PdVM
    {
        public string PdId { get; set; } = null!;

        public string PdName { get; set; } = null!;

        public string? PdtName { get; set; }

        public string? CategoryName { get; set; }

        public double? PdPrice { get; set; }

        public double? PdCost { get; set; }

        public double? PdStk { get; set; }

        public string PdDs { get; set; } = null!;

        
    }
}
