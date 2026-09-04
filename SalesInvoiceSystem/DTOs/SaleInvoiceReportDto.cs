namespace SalesInvoiceSystem.DTOs
{
    public class SaleInvoiceReportDto
    {
        public long SaleId { get; set; }

        public string InvoiceNo { get; set; } = string.Empty;

        public DateTime SaleDate { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerAddress { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal InvoiceTotal { get; set; }
    }
}
