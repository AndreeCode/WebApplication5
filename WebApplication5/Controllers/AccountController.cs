using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;
using WebApplication5.ViewModels;

namespace WebApplication5.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, Location = model.Location, PhoneNumber = model.PhoneNumber, PhoneNumberPublic = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Inicio de sesión inválido");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var model = new ManageViewModel { Email = user.Email, FullName = user.FullName, Location = user.Location, PhoneNumberPublic = user.PhoneNumberPublic };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(ManageViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            user.FullName = model.FullName;
            user.Location = model.Location;
            user.PhoneNumberPublic = model.PhoneNumberPublic;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ViewBag.Message = "Perfil actualizado";
                return View(model);
            }
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }
    }
}
