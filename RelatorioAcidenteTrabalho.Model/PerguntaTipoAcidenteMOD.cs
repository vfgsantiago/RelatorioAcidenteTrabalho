namespace RelatorioAcidenteTrabalho.Model
{
    public class PerguntaTipoAcidenteMOD
    {
        public int CdPerguntaTipoAcidente { get; set; }
        public int CdTipoAcidente { get; set; }
        public string TxTipoAcidente { get; set; }
        public int CdPergunta { get; set; }
        public string TxPergunta { get; set; }
        public int NrOrdemExibicao { get; set; }
    }
}
