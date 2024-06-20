namespace KuShop.ViewModels
{
    public class RepStkTran
    {
        public string PdId { get; set; } = null!;
        public string PdName { get; set; } = null!;

        //วันที่มีการเคลื่อนไหว Stock
        public DateTime? TranDate { get; set; }
        //ประเภทของการย้าย Stock IN,OUT
        public string TranType { get; set; }

        //จำนวนที่ย้าย
        public double? TranQty { get; set; }
    }
}
