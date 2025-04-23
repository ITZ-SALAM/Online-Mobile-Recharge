using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MobileRecharge.Data;
using MobileRecharge.Models;

namespace MobileRecharge.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.AppUser> _userManager; // IdentityUser ki jagah AppUser

        public AccountController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity/Account");
            }

            var transactions = _context.Transactions
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.Date)
                .ToList();

            ViewBag.UserEmail = user.Email;
            return View(transactions);
        }

        [HttpGet]
        public IActionResult Recharge()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recharge(string mobileNumber, string planType, decimal amount)
        {
            if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(planType) || amount <= 0)
            {
                ModelState.AddModelError("", "Please provide valid mobile number, plan type, and amount.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity/Account");
            }

            var transaction = new Transaction
            {
                MobileNumber = mobileNumber.Trim(),
                PlanType = planType.Trim(),
                Amount = amount,
                TransactionId = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                UserId = user.Id
            };

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Recharge of Rs. {amount} for {mobileNumber} completed successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the transaction: " + ex.Message);
                return View();
            }

            return RedirectToAction("MyAccount");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity/Account");
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found or you don't have permission to delete it.";
                return RedirectToAction("MyAccount");
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Transaction deleted successfully!";
            return RedirectToAction("MyAccount");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity/Account");
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult PostBillPayment()
        {
            return View(new PostBillPaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostBillPayment(PostBillPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            string userId = user != null ? user.Id : "Guest";

            var transaction = new Transaction
            {
                MobileNumber = model.MobileNumber,
                PlanType = "Postpaid Bill Payment",
                Amount = model.Amount,
                TransactionId = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                UserId = userId
            };

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bill payment of Rs. {model.Amount} for {model.MobileNumber} completed successfully!";
                return RedirectToAction("MyAccount");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity/Account");
            }

            // Update email
            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            // Update username
            if (user.UserName != model.UserName)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("MyAccount");
        }
    }

    public class ProfileViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}