namespace RelatorioAcidenteTrabalho.Model
{
    public class AcidenteMOD
    {
        public int CdAcidente { get; set; }
        public int CdTipoAcidente { get; set; }
        public string TxTipoAcidente { get; set; }
        public string TxTitulo { get; set; }
        public string? TxObservacao { get; set; }
        public DateTime DtAcidente{ get; set; }
        public int CdAcidentado { get; set; }
        public string TxNome { get; set; }
        public int NrMatricula { get; set; }
        public DateTime DtRegistro { get; set; }
        public int CdUsuarioRegistrou { get; set; }
        public string TxUsuarioRegistrou { get; set; }
    }
}
