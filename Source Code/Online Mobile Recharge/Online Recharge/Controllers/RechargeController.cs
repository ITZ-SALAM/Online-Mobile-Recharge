using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileRecharge.Data;
using MobileRecharge.Models;
using Online_Recharge.Data;
using System;

namespace MobileRecharge.Controllers
{
    public class RechargeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RechargeController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult OnlineRecharge()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OnlineRecharge(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber) || mobileNumber.Length != 10 || !mobileNumber.All(char.IsDigit))
            {
                ViewBag.ErrorMessage = "Please enter a valid 10-digit mobile number (no more, no less).";
                return View();
            }
            return RedirectToAction("RechargeOptions", new { mobileNumber = mobileNumber });
        }

        [HttpGet]
        public IActionResult RechargeOptions(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber))
            {
                return RedirectToAction("OnlineRecharge");
            }
            ViewBag.MobileNumber = mobileNumber;
            return View();
        }

        [HttpGet]
        public IActionResult Payment(string mobileNumber, string planType, string amount)
        {
            if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(planType) || string.IsNullOrEmpty(amount))
            {
                return RedirectToAction("OnlineRecharge");
            }

            ViewBag.MobileNumber = mobileNumber;
            ViewBag.PlanType = planType;
            ViewBag.Amount = amount;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string mobileNumber, string planType, string amount, string cardNumber, string expiry, string cvv)
        {
            if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(expiry) || string.IsNullOrEmpty(cvv))
            {
                ViewBag.ErrorMessage = "Please fill all payment details.";
                ViewBag.MobileNumber = mobileNumber;
                ViewBag.PlanType = planType;
                ViewBag.Amount = amount;
                return View("Payment");
            }

            var user = await _userManager.GetUserAsync(User);
            string userId = user != null ? user.Id : "Guest"; // Guest user support

            var transaction = new Transaction
            {
                MobileNumber = mobileNumber,
                PlanType = planType,
                Amount = decimal.Parse(amount), // Assuming amount is valid
                TransactionId = "TXN" + DateTime.Now.Ticks, // Dummy transaction ID
                Date = DateTime.Now,
                UserId = userId
            };

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("TransactionReceipt", new { mobileNumber, planType, amount });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while processing the payment: " + ex.Message;
                ViewBag.MobileNumber = mobileNumber;
                ViewBag.PlanType = planType;
                ViewBag.Amount = amount;
                return View("Payment");
            }
        }

        [HttpGet]
        public IActionResult TransactionReceipt(string mobileNumber, string planType, string amount)
        {
            if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(planType) || string.IsNullOrEmpty(amount))
            {
                return RedirectToAction("OnlineRecharge");
            }

            ViewBag.MobileNumber = mobileNumber;
            ViewBag.PlanType = planType;
            ViewBag.Amount = amount;
            ViewBag.TransactionId = "TXN" + DateTime.Now.Ticks; // Consistent with ProcessPayment
            return View();
        }
    }
}