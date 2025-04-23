using System.ComponentModel.DataAnnotations;

namespace MobileRecharge.Models
{
    public class PostBillPaymentViewModel
    {
        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Mobile number must contain only digits.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}