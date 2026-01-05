namespace RelatorioAcidenteTrabalho.Model
{
    public class AcidentadoMOD
    {
        public int CdAcidentado { get; set; }
        public int? CdUsuario { get; set; }
        public int NrMatricula { get; set; }
        public string TxNome { get; set; }
        public int? CdFuncao { get; set; }
        public string TxFuncao { get; set; }
        public int? CdUnidade { get; set; }
        public string TxUnidade { get; set; }
        public int? CdCargo { get; set; }
        public string TxCargo { get; set; }
        public int? CdCentroCusto { get; set; }
        public string TxCentroCusto { get; set; }
        public string TxTelefone { get; set; }
        public int QtdAcidente { get; set; }
    }
}
