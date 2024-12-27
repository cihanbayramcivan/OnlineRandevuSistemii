using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Enums;
using OnlineRandevuSistemi.Models;

[Authorize(Roles = "Admin")] // Admin rolüyle giriş yapan kullanıcılar için erişim izni
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context; // Veritabanı bağlamı (DB Context) ile ilişki kurulur
    }

    // Admin paneli ana sayfası
    public IActionResult Index()
    {
        return View(); // Admin ana sayfasını render eder
    }

    // Çıkış işlemi
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Kullanıcı oturumu temizlenir
        TempData["Success"] = "Başarıyla çıkış yaptınız."; // Başarı mesajı eklenir
        return RedirectToAction("Login", "Account"); // Login sayfasına yönlendirilir
    }

    // Kullanıcıları listeleme
    public IActionResult ManageUsers()
    {
        var users = _context.Users.Include(u => u.Role) // Kullanıcılar ve rollerini dahil eder
            .Select(u => new UserViewModel // Kullanıcıları ViewModel'e dönüştürür
            {
                Id = u.Id,
                Username = u.Username,
                RoleName = u.Role.Name
            }).ToList();

        return View(users); // Kullanıcı listesini görüntüler
    }

    // Kullanıcı oluşturma (GET)
    [HttpGet]
    public IActionResult CreateUser()
    {
        var model = new User(); // Yeni bir User modeli oluşturulur

        // Rolleri dropdown listesi olarak View'e gönderir
        ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();

        return View(model); // Kullanıcı oluşturma formunu render eder
    }

    // Kullanıcı oluşturma (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateUser(UserViewModel model)
    {
        // Alan doğrulamaları yapılır
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.PasswordHash) || model.RoleId == 0)
        {
            TempData["Error"] = "Lütfen tüm alanları eksiksiz doldurun."; // Hata mesajı
            return RedirectToAction("CreateUser"); // Hata durumunda formu tekrar gösterir
        }

        try
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash); // Şifre hash'lenir

            // Yeni kullanıcı oluşturulur
            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                RoleId = model.RoleId
            };

            _context.Users.Add(newUser); // Kullanıcı veritabanına eklenir
            _context.SaveChanges(); // Değişiklikler kaydedilir

            TempData["Success"] = "Kullanıcı başarıyla oluşturuldu!"; // Başarı mesajı
            return RedirectToAction("ManageUsers"); // Kullanıcılar sayfasına yönlendirilir
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Kullanıcı oluşturulurken bir hata oluştu: {ex.Message}"; // Hata mesajı
            ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList(); // Rolleri yeniden gönderir
            return View(model); // Formu tekrar render eder
        }
    }

    // Kullanıcı silme
    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id); // Kullanıcıyı bulur
        if (user != null)
        {
            _context.Users.Remove(user); // Kullanıcı veritabanından silinir
            _context.SaveChanges(); // Değişiklikler kaydedilir
            TempData["Success"] = "Kullanıcı başarıyla silindi!"; // Başarı mesajı
        }
        else
        {
            TempData["Error"] = "Kullanıcı bulunamadı."; // Hata mesajı
        }

        return RedirectToAction("ManageUsers"); // Kullanıcılar sayfasına yönlendirir
    }

    // Randevuları listeleme
    public IActionResult ManageAppointments()
    {
        var appointments = _context.Appointments
            .Include(a => a.User) // Randevuların kullanıcılarıyla birlikte getirilmesi
            .Include(a => a.Service) // Randevuların hizmetleriyle birlikte getirilmesi
            .ToList();

        return View(appointments); // Randevu listesini görüntüler
    }

    // Randevu durumunu güncelleme (GET)
    public IActionResult UpdateAppointmentStatus(int id)
    {
        var appointment = _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstOrDefault(a => a.Id == id); // Randevuyu bulur

        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı."; // Hata mesajı
            return RedirectToAction("ManageAppointments"); // Randevular sayfasına yönlendirir
        }

        var model = new AppointmentViewModel
        {
            Id = appointment.Id,
            Status = appointment.Status
        };

        return View(model); // Randevu durumunu güncelleme formunu render eder
    }

    // Randevu durumunu güncelleme (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAppointmentStatus(AppointmentViewModel model)
    {
        var appointment = _context.Appointments.FirstOrDefault(a => a.Id == model.Id); // Randevuyu bulur
        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı."; // Hata mesajı
            return RedirectToAction("ManageAppointments"); // Randevular sayfasına yönlendirir
        }

        appointment.Status = model.Status; // Durum güncellenir
        _context.Appointments.Update(appointment); // Randevu güncellenir
        _context.SaveChanges(); // Değişiklikler kaydedilir

        TempData["Success"] = "Randevu durumu başarıyla güncellendi!"; // Başarı mesajı
        return RedirectToAction("ManageAppointments"); // Randevular sayfasına yönlendirir
    }

    // Kullanıcı rolünü güncelleme (GET)
    [HttpGet]
    public IActionResult UpdateUserRole(int id)
    {
        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id); // Kullanıcıyı ve rolünü getirir
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı."; // Hata mesajı
            return RedirectToAction("ManageUsers"); // Kullanıcılar sayfasına yönlendirir
        }

        ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList(); // Rolleri dropdown listesi olarak View'e gönderir

        var model = new UserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = user.Role.Name
        };

        return View(model); // Kullanıcı rolü güncelleme formunu render eder
    }

    // Kullanıcı rolünü güncelleme (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateUserRole(UserViewModel model)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == model.Id); // Kullanıcıyı bulur
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı."; // Hata mesajı
            return RedirectToAction("ManageUsers"); // Kullanıcılar sayfasına yönlendirir
        }

        user.RoleId = model.RoleId; // Kullanıcı rolü güncellenir
        _context.Users.Update(user); // Kullanıcı güncellenir
        _context.SaveChanges(); // Değişiklikler kaydedilir

        TempData["Success"] = "Kullanıcı rolü başarıyla güncellendi!"; // Başarı mesajı
        return RedirectToAction("ManageUsers"); // Kullanıcılar sayfasına yönlendirir
    }
}
