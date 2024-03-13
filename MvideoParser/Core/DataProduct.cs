namespace MvideoParser.Core
{
    public class DataProduct
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Percent { get; set; }
        public decimal BonusRubles { get; set; }
        public byte[] ProductCardByteArr { get; set; }
    }
}
