namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidenteDetalheViewMOD
    {
        public int CdAcidente { get; set; }
        public string TxTitulo { get; set; }
        public string TxObservacao { get; set; }
        public DateTime DtAcidente { get; set; }
        public string TxTipoAcidente { get; set; }
        public int CdAcidentado { get; set; }
        public string TxNome { get; set; }
        public int NrMatricula { get; set; }
        public string TxTelefone { get; set; }
        public string TxCentroCusto { get; set; }
        public string TxUnidade { get; set; }
        public string TxCargo { get; set; }
        public string TxFuncao { get; set; }
        public string TxUsuarioRegistrou { get; set; }
        public DateTime DtRegistro { get; set; }
        public List<PerguntaRespostaMOD> ListaPerguntaResposta { get; set; } = new();
    }
}
