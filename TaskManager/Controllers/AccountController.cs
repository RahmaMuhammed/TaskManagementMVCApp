using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Threading.Tasks;
using TaskManager.Helpers;
using TaskManager.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, ICustomEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new AppUser instance from the RegisterVM
                var user = new AppUser
                {
                    UserName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CompletedTaskCount = 0,
                    TaskCount = 0
                };
                // Create the user in the database with the given password
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Build the email confirmation link
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, token = token }, Request.Scheme);

                    // Send confirmation email to the user
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your email by clicking here: <a href='{confirmationLink}'>Confirm Email</a>");

                    // Assign role based on email domain
                    if (model.Email.EndsWith("@taskmanager.admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }


                    return View("EmailConfirmationNotice");
                }
                else
                {
                    // Show validation errors if user creation failed
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                }
            }
            // If we got this far, something failed; redisplay form
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // Check if the user ID or token is missing from the URL
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                // Show an error message if the link is invalid
                ViewData["ErrorMessage"] = "Invalid email confirmation link.";
                return View("Error");
            }
            // Try to find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Show an error message if the user is not found
                ViewData["ErrorMessage"] = "The user could not be found.";
                return View("Error");
            }
            // Confirm the user's email using the token
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // If confirmation succeeded, show success view
                return View("ConfirmEmailSuccess");
            }
            else
            {
                // If confirmation failed, show an error message
                ViewData["ErrorMessage"] = "Email confirmation failed. Please try again.";
                return View("Error");
            }
        }

    }
}
