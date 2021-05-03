using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistrationProject.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationProject.Controllers
{
    public class AuthController : Controller
    {
        private IConfiguration Configuration { get; set; }

        public AuthController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginInfo, [FromQuery] string returnUrl)
        {


            if (ModelState.IsValid)
            {
                // Use Input.Email and Input.Password to authenticate the user
                // with your custom authentication logic.
                //
                // For demonstration purposes, the sample validates the user
                // on the email address maria.rodriguez@contoso.com with 
                // any password that passes model validation.

                ApplicationUser user = await AuthenticateUser(loginInfo.Email, loginInfo.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View();
                }
                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim(ClaimTypes.Role, "Administrator"),
                };

                ClaimsIdentity claimsIdentity = new(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties authProperties = new()
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);




                if (!Url.IsLocalUrl(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }


                return LocalRedirect(returnUrl);
            }
            return View();


            // Something failed. Redisplay the form.

        }

        private async Task<ApplicationUser> AuthenticateUser(string email, string password)
        {
            // For demonstration purposes, authenticate a user
            // with a static email address. Ignore the password.
            // Assume that checking the database takes 500ms

            await Task.Delay(500);

            if (email == Configuration.GetSection("AdminEmail").Value && ToSha256(password) == Configuration.GetSection("AdminPassword").Value)
            {


                return new ApplicationUser(email, "Admin");


            }
            return null;

        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private static string ToSha256(string randomString)
        {
            SHA256Managed crypt = new();
            string hash = string.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }
}
