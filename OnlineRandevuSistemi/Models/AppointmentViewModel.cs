using OnlineRandevuSistemi.Enums;

namespace OnlineRandevuSistemi.Models
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public AppointmentStatus Status { get; set; } // Enum tipi
    }
}
