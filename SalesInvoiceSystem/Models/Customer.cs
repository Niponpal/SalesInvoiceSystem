using System.ComponentModel.DataAnnotations;

namespace SalesInvoiceSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, MinimumLength = 2,
                 ErrorMessage = "Customer name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s.'-]+$",
                 ErrorMessage = "Customer name can contain only letters, spaces, dots, apostrophes and hyphens.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(?:\+8801|01)[3-9]\d{8}$",
           ErrorMessage = "Enter a valid Bangladesh phone number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150,
             ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, MinimumLength = 5,
            ErrorMessage = "Address must be between 5 and 250 characters.")]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation Property
        public ICollection<Sale>? Sales { get; set; }
    }
}
