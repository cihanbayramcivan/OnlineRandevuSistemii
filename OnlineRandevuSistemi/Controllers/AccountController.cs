using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Models;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return user.Role.Name switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "User" => RedirectToAction("Index", "Appointments"),
            _ => RedirectToAction("Login")
        };
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();

        return View(new UserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(UserViewModel model)
    {
        try
        {
            var userRole = _context.Roles.FirstOrDefault(r => r.Name == "User");
            if (userRole == null)
            {
                TempData["Error"] = "Varsayılan kullanıcı rolü bulunamadı. Lütfen yöneticinize başvurun.";
                return View(model);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);

            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                RoleId = userRole.Id
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["Success"] = "Kayıt başarılı! Artık giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Kayıt sırasında bir hata oluştu: {ex.Message}";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
}
