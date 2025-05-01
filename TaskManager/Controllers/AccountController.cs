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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;
using System.Web;

namespace TaskManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomEmailSender _emailSender;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, ICustomEmailSender emailSender, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        ///////////////////////////////////////// SIGNUP ////////////////////////////////////////////////
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

        ///////////////////////////////////////// SIGNIN ////////////////////////////////////////////////
        public async Task<IActionResult> Login()
        {
            return View();
        }

       [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Try to find the user by their email address
                var user = await _userManager.FindByEmailAsync(model.UserEmail);
                if (user is null)
                {
                    // Show an error message if the user is not found
                    ViewData["ErrorMessage"] = "The user could not be found.";
                        return View("Error"); 
                }
                // Check if the user's email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    // If not confirmed, resend confirmation email
                    await ResendConfirmationEmail(model.UserEmail);
                }

                // Attempt to sign in the user with the provided password
                var result = await _signInManager.CheckPasswordSignInAsync(user,model.Password,false);
                if(result.Succeeded)
                {
                    // Sign the user in by creating an authentication cookie after successful password check
                    // 'isPersistent: false' means the user will be signed out when the browser is closed
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    // If login succeeded, redirect to the home page
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    // If login failed, show a validation error
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);

        }

        // Show forgot password form
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Handle forgot password form submission
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Show confirmation view even if user not found (security reasons)
                    return View("ForgotPasswordConfirmation");
                }

                // Check if email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Show email not confirmed message
                    return View("EmailNotConfirmed");
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Encode the token for URL safety
                var encodedToken = HttpUtility.UrlEncode(token);

                // Build the reset link
                var resetLink = Url.Action("ResetPassword", "Account", new {token = encodedToken, email = user.Email }, Request.Scheme);

                // Send reset email
                await _emailSender.SendEmailAsync(user.Email, "Reset Password",
                    $"Click to reset your password: <a href='{resetLink}'>Reset</a>");

                // Show confirmation page
                return View("ForgotPasswordConfirmation");
            }

            // If form is invalid, redisplay it
            return View(model);
        }

        // POST: Resend email confirmation link to user
        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewData["ErrorMessage"] = "Invalid email address.";
                return View("Error");
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View("ForgotPasswordConfirmation");
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Build confirmation link
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token = token }, Request.Scheme);

            // Send confirmation email
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your email by clicking here: <a href='{confirmationLink}'>Confirm Email</a>");

            // Show confirmation notice
            return View("EmailConfirmationNotice");
        }

        // GET: Display reset password form with token and email
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            // Create model and pass it to view
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: Handle reset password form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }

            // Decode token from URL
            model.Token = HttpUtility.UrlDecode(model.Token);

            // Try to reset password
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            // Show validation errors if reset failed
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Redisplay form with errors
            return View(model);
        }

        ///////////////////////////////////////// SIGNOUT ////////////////////////////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
