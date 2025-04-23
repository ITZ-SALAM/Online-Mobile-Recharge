using Microsoft.AspNetCore.Identity;

namespace MobileRecharge.Models
{
    public class AppUser : IdentityUser
    {
        public string? PhoneNumber { get; set; }
    }
}