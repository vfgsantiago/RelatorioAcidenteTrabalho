using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidenteEnvioViewMOD
    {
        public int CdTipoAcidente { get; set; }
        public int CdUsuario { get; set; }
        public int CdUsuarioAcidentado { get; set; }
        public int NrMatricula { get; set; }
        public string TxNome { get; set; }
        public int? CdFuncao { get; set; }
        public int? CdUnidade { get; set; }
        public int? CdCargo { get; set; }
        public int? CdCentroCusto { get; set; }
        public string TxTelefone { get; set; }
        public string TxTitulo { get; set; }
        public string? TxObservacao { get; set; }
        public DateTime DtAcidente { get; set; }
        public List<RespostaItemViewMOD> ListaResposta { get; set; }
    }
}
