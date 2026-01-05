namespace RelatorioAcidenteTrabalho.Model
{
    public class PerguntaMOD
    {
        public int CdPergunta { get; set; }
        public string TxPergunta { get; set; }
        public DateTime DtCadastro { get; set; }
        public int CdUsuarioCadastrou { get; set; }
        public DateTime? DtAlteracao { get; set; }
        public int? CdUsuarioAlterou { get; set; }
        public string NoUsuarioAlterou { get; set; }
        public string SnAtivo { get; set; }
    }
}
