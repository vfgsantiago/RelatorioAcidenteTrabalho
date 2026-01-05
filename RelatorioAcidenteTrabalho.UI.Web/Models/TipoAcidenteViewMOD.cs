using System.ComponentModel.DataAnnotations;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class TipoAcidenteViewMOD
    {
        [Display(Name = "Código do Tipo de Acidente")]
        public Int32 CdTipoAcidente { get; set; }

        [Display(Name = "Tipo de Acidente")]
        public string TxTipoAcidente { get; set; }

        [Display(Name = "Descrição")]
        public string TxDescricao { get; set; }

        [Display(Name = "Status")]
        public string SnAtivo { get; set; }

        [Display(Name = "Usuário que cadastrou")]
        public Int32 CdUsuarioCadastrou { get; set; }

        [Display(Name = "Data de cadastro")]
        public DateTime DtCadastro { get; set; }

        [Display(Name = "Usuário que alterou")]
        public Int32 CdUsuarioAlterou { get; set; }

        [Display(Name = "Usuário que alterou")]
        public string NoUsuarioAlterou { get; set; }

        [Display(Name = "Data de alteração")]
        public DateTime DtAlteracao { get; set; }
        public List<TipoAcidenteMOD> ListaTipoAcidente { get; set; } = new List<TipoAcidenteMOD>();
        public int QtdTotalDeRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
