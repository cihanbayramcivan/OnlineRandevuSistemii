using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Enums;
using OnlineRandevuSistemi.Models;

[Authorize(Roles = "User")]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Randevuların listelendiği Index sayfası
    public IActionResult Index()
    {
        try
        {
            // Kullanıcı adını Claims'den al
            var username = HttpContext.User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Kullanıcı bilgileri alınamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcıya ait randevuları getir
            var appointments = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.User)
                .Where(a => a.User.Username == username)
                .ToList();

            return View(appointments);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Randevular yüklenirken bir hata oluştu: {ex.Message}";
            return RedirectToAction("Error");
        }
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(); // Oturumu sonlandır
        TempData["Success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Login", "Account");
    }

    // Yeni randevu oluşturma sayfası (GET)
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            ViewBag.Services = _context.Services.AsNoTracking()
                .Select(s => new { s.Id, s.Name })
                .ToList();

            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Randevu oluşturma sayfası yüklenirken bir hata oluştu: {ex.Message}";
            return RedirectToAction("Error");
        }
    }

    // Yeni randevu oluşturma işlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Appointment appointment)
    {
        try
        {
            if (appointment.ServiceId == null || appointment.ServiceId == 0)
            {
                TempData["Error"] = "Lütfen bir hizmet seçin.";
                ViewBag.Services = _context.Services.AsNoTracking()
                    .Select(s => new { s.Id, s.Name })
                    .ToList();
                return View(appointment);
            }

            // Kullanıcı adını Claims'den al
            var username = HttpContext.User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Login", "Account");
            }

            appointment.Status = AppointmentStatus.Bekliyor;
            appointment.UserId = user.Id;
            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Randevu başarıyla oluşturuldu!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Randevu kaydedilirken bir hata oluştu: {ex.Message}";
            ViewBag.Services = _context.Services.AsNoTracking()
                .Select(s => new { s.Id, s.Name })
                .ToList();
            return View(appointment);
        }
    }

    // Randevuyu düzenleme sayfası (GET)
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            TempData["Error"] = "Geçersiz randevu kimliği.";
            return RedirectToAction("Index");
        }

        try
        {
            var appointment = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                TempData["Error"] = "Güncellenecek randevu bulunamadı.";
                return RedirectToAction("Index");
            }

            LoadDropdownData();
            return View(appointment);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Düzenleme sayfası yüklenirken bir hata oluştu: {ex.Message}";
            return RedirectToAction("Error");
        }
    }

    // Randevuyu düzenleme işlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Appointment appointment)
    {
        try
        {
            if (appointment.ServiceId == null || appointment.ServiceId == 0)
            {
                TempData["Error"] = "Lütfen bir hizmet seçin.";
                LoadDropdownData();
                return View(appointment);
            }

            var existingAppointment = _context.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
            if (existingAppointment == null)
            {
                TempData["Error"] = "Güncellenmek istenen randevu bulunamadı.";
                return RedirectToAction("Index");
            }     

            existingAppointment.ServiceId = appointment.ServiceId;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.Status = appointment.Status;

            _context.Appointments.Update(existingAppointment);
            _context.SaveChanges();
            TempData["Success"] = "Randevu başarıyla güncellendi!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Güncelleme sırasında bir hata oluştu: {ex.Message}";
            LoadDropdownData();
            return View(appointment);
        }
    }

    // Randevuyu silme işlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                TempData["Error"] = "Silinmek istenen randevu bulunamadı.";
                return RedirectToAction("Index");
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            TempData["Success"] = "Randevu başarıyla silindi!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Randevu silinirken bir hata oluştu: {ex.Message}";
            return RedirectToAction("Error");
        }
    }

    // Hata sayfası
    public IActionResult Error()
    {
        return View();
    }

    // Dropdown verilerini yüklemek için yardımcı metot
    private void LoadDropdownData()
    {
        ViewBag.Users = _context.Users.AsNoTracking().ToList();
        ViewBag.Services = _context.Services.AsNoTracking().ToList();
    }
}
