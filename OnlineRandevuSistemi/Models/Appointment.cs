using OnlineRandevuSistemi.Enums;
using OnlineRandevuSistemi.Models;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; }

    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } // Enum tipi
}
