namespace RelatorioAcidenteTrabalho.Model
{
    public class RespostaMOD
    {
        public int CdResposta { get; set; }
        public int CdPergunta { get; set; }
        public string TxPergunta { get; set; }
        public string TxResposta { get; set; }
        public DateTime DtResposta { get; set; }
        public int CdAcidente { get; set; }
        public int CdUsuarioRespondeu { get; set; }
    }
}
