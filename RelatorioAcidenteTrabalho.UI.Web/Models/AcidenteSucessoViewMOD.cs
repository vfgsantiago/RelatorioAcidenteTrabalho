namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidenteSucessoViewMOD
    {
        public int CdAcidente { get; set; }
        public string TxTitulo { get; set; }
        public string TxObservacao { get; set; }
        public DateTime DtAcidente { get; set; }
        public string TipoAcidente { get; set; }
        public string TxNome { get; set; }
        public int NrMatricula { get; set; }
        public string TxTelefone { get; set; }
        public string CentroCusto { get; set; }
        public string Unidade { get; set; }
        public string Cargo { get; set; }
        public string Funcao { get; set; }
        public string TxUsuarioRegistrou { get; set; }
        public DateTime DtRegistro { get; set; }
        public List<PerguntaRespostaMOD> ListaPerguntaResposta { get; set; } = new();
    }

    public class PerguntaRespostaMOD
    {
        public string Pergunta { get; set; }
        public string Resposta { get; set; }
    }
}
