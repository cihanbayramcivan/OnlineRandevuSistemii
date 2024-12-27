using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı 50 karakterden fazla olamaz.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifre 100 karakterden fazla olamaz.")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Lütfen bir rol seçin.")]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
