using Microsoft.EntityFrameworkCore;  // Entity Framework Core kütüphanesini dahil eder
using OnlineRandevuSistemi.Models;
using System.Data;  // ADO.NET ile ilgili sınıfları içerir, ancak burada kullanılmaz

// ApplicationDbContext sınıfı, veritabanı ile etkileşimi yöneten sınıftır
public class ApplicationDbContext : DbContext
{
    // DbContext sınıfının yapıcı metodu, veritabanı bağlantı seçeneklerini alır
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Veritabanındaki Role tablosunu temsil eder
    public DbSet<Role> Roles { get; set; }

    // Veritabanındaki User tablosunu temsil eder
    public DbSet<User> Users { get; set; }

    // Veritabanındaki Service tablosunu temsil eder
    public DbSet<Service> Services { get; set; }

    // Veritabanındaki Appointment tablosunu temsil eder
    public DbSet<Appointment> Appointments { get; set; }
}
