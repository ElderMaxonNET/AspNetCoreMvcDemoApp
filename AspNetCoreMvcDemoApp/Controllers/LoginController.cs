using AspNetCoreMvcDemoApp.Core.Common;
using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SadLib.Security.Extensions;
using System.Security.Claims;

namespace AspNetCoreMvcDemoApp.Controllers
{
    public class LoginController(IWebHostEnvironment env, IConfiguration configuration, IDataService dataService) : Controller
    {
        private async Task<IActionResult> PerformLoginAsync(UserLoginDto model)
        {
            var user = await dataService.Users.GetAsync(model.Email);
            if (user == null || !model.Password.VerifyHashBytes(user.PwHash, user.PwSalt, HashType.SHA512))
            {
                ModelState.AddModelError(nameof(model.Password), "Invalid login.");
                return View("Index", new UserLoginDto { Email = model.Email });
            }

            RoleTypes role = (RoleTypes)user.RoleId;
            string? redirectUrl = role switch
            {
                RoleTypes.Admin => Url.Action("Index", "Users"),
                RoleTypes.Uploader => Url.Action("Index", "UserFiles"),
                RoleTypes.Downloader => Url.Action("Index", "Download"),
                _ => null
            } ?? throw new Exception($"{role} not found.");

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Email, user.Email),
                new (ClaimTypes.Name, $"{user.Name} {user.Surname}"),
                new (ClaimTypes.Role, role.ToString()),
                new ("RoleId", user.RoleId.ToString()),
                new ("Avatar", user.Avatar)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = model.RememberMe });

            return Redirect(redirectUrl);
        }


        [HttpGet]
        public async Task<IActionResult> Index(bool disableAutoLogin = false)
        {
            // Auto-login for development environment and auto-login is not disabled
            if (env.IsDevelopment() && !disableAutoLogin)
            {
                // Attempt auto-login using credentials from configuration (e.g., UserSecrets secrets.json, appsettings.Development.json)
                string? email = configuration["DefaultLogin:Email"];
                string? password = configuration["DefaultLogin:Password"];
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    return await PerformLoginAsync(new UserLoginDto { Email = email, Password = password, RememberMe = true });
                }
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return await PerformLoginAsync(model);
        }

        public async Task<IActionResult> Logout(bool disableAutoLogin = false)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", new { disableAutoLogin });
        }

    }
}
