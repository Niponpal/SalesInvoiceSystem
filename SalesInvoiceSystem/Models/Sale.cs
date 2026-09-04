using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesInvoiceSystem.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNo { get; set; }

        public int CustomerId { get; set; }

        public DateTime SaleDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

   
        public ICollection<SaleDetail> SaleDetails { get; set; }
           = new List<SaleDetail>();

    }
}
