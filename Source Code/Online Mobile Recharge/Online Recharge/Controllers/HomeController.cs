using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Online_Recharge.Models;
using MobileRecharge.Models;
using MobileRecharge.Data;

namespace MobileRecharge1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Feedback()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(Feedback model)
        {
            model.UserId = User.Identity!.IsAuthenticated ? (await _userManager.GetUserAsync(User)).Id : null!;
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thank you for your feedback!";
                return RedirectToAction("Feedback");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult SiteMap()
        {
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                _context.Contacts.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving your message: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult CustomerCare()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}