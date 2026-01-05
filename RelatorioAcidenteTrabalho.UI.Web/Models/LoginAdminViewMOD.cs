using System.ComponentModel.DataAnnotations;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class LoginAdminViewMOD
    {
        public int? Id { get; set; }

        public string? Nome { get; set; }

        [Required(ErrorMessage = "Preencha seu login", AllowEmptyStrings = false)]
        [Display(Name = "Login")]
        public string? Login { get; set; } = null;

        [Required(ErrorMessage = "Preencha sua senha", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null;

        public bool chkLembrar { get; set; }
    }
}
