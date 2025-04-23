using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileRecharge.Data;
using MobileRecharge.Models;
using Online_Recharge.Data;

namespace MobileRecharge.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string mobileNumber, DateTime? startDate, DateTime? endDate)
        {
            var transactions = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                transactions = transactions.Where(t => t.MobileNumber.Contains(mobileNumber));
            }

            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.Date <= endDate.Value);
            }

            var filteredTransactions = await transactions
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            ViewBag.MobileNumber = mobileNumber;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(filteredTransactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction("Index");
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Transaction deleted successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers(string searchEmail)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchEmail))
            {
                users = users.Where(u => u.Email.Contains(searchEmail) || u.UserName.Contains(searchEmail));
            }

            var filteredUsers = await users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in filteredUsers)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewBag.UserRoles = userRoles;
            ViewBag.SearchEmail = searchEmail;

            return View(filteredUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot delete yourself.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "User is already an admin.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Admin role added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error adding admin role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdminRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot remove your own admin role.";
                return RedirectToAction("ManageUsers");
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "User is not an admin.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Admin role removed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error removing admin role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public async Task<IActionResult> ViewFeedback()
        {
            var feedbacks = await _context.Feedbacks
                .OrderByDescending(f => f.SubmittedOn)
                .Select(f => new FeedbackViewModel // Optional improvement
                {
                    Id = f.Id,
                    Name = f.Name,
                    Message = f.Message,
                    SubmittedOn = f.SubmittedOn ?? DateTime.MinValue,
                    UserEmail = f.UserId != null ? _context.Users.Where(u => u.Id == f.UserId).Select(u => u.Email).FirstOrDefault() : "Guest"
                })
                .ToListAsync();
            return View(feedbacks);
        }
    }

    // Optional ViewModel for ViewFeedback
    public class FeedbackViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string UserEmail { get; set; }
    }
}