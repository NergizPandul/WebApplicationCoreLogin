using System.ComponentModel.DataAnnotations;

namespace WebApplicationCoreLogin.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı zorunludur")]
        [StringLength(30,ErrorMessage ="Kullanıcı adı max. 30 karakter olmalıdır")]
        public string UserName { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(16)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
