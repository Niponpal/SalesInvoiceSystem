using System.ComponentModel.DataAnnotations.Schema;

namespace SalesInvoiceSystem.Models
{
    public class SaleDetail
    {
        public int Id { get; set; }

        public int SaleId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation Properties
        [ForeignKey("SaleId")]
        public Sale Sale { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
