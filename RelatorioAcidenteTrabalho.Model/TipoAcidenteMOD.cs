namespace RelatorioAcidenteTrabalho.Model
{
    public class TipoAcidenteMOD
    {
        public int CdTipoAcidente { get; set; }
        public string TxTipoAcidente { get; set; }
        public string TxDescricao { get; set; }
        public DateTime DtCadastro { get; set; }
        public int CdUsuarioCadastrou { get; set; }
        public DateTime? DtAlteracao { get; set; }
        public int? CdUsuarioAlterou { get; set; }
        public string NoUsuarioAlterou { get; set; }
        public string SnAtivo { get; set; }
    }
}
