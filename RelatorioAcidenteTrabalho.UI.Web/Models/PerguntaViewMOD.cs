using System.ComponentModel.DataAnnotations;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class PerguntaViewMOD
    {
        [Display(Name = "Código da Pergunta")]
        public Int32 CdPergunta { get; set; }

        [Display(Name = "Pergunta")]
        public string TxPergunta { get; set; }

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
        public List<PerguntaMOD> ListaPergunta { get; set; } = new List<PerguntaMOD>();
        public int QtdTotalDeRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
