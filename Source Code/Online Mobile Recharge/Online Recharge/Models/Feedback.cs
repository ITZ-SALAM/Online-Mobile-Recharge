using System.ComponentModel.DataAnnotations;

namespace MobileRecharge.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters.")]
        public string Message { get; set; }

        public DateTime? SubmittedOn { get; set; } = DateTime.Now; // Nullable, to handle different scenarios

        public string UserId { get; set; } // Nullable for guest users
    }
}
