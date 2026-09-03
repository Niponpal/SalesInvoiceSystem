using System.ComponentModel.DataAnnotations;

namespace SalesInvoiceSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation Property
        public ICollection<Sale>? Sales { get; set; }
    }
}
