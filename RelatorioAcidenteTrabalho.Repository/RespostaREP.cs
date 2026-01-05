using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class RespostaREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public RespostaREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorAcidente
        public async Task<List<RespostaMOD>> BuscarPorAcidente(int cdAcidente)
        {
            using var con = new OracleConnection(_conexaoOracle);
            var query = @"SELECT R.CD_RESPOSTA,
                                       R.CD_ACIDENTE,
                                       R.CD_PERGUNTA,
                                       P.TX_PERGUNTA,
                                       R.TX_RESPOSTA,
                                       R.DT_RESPOSTA,
                                       R.CD_USUARIO_RESPONDEU
                                  FROM RAT_RESPOSTA R, RAT_PERGUNTA P
                                 WHERE R.CD_PERGUNTA = P.CD_PERGUNTA
                                   AND R.CD_ACIDENTE = :CdAcidente
                                 ORDER BY R.CD_PERGUNTA";

            var parametros = new DynamicParameters();
            parametros.Add("CdAcidente", cdAcidente, DbType.Int32);

            var lista = (await con.QueryAsync<RespostaMOD>(query, parametros)).ToList();
            return lista;
        }
        #endregion

        #region Inserir
        public async Task<bool> Inserir(RespostaMOD resposta)
        {
            bool inseriu = false;
            using (var con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO RAT_RESPOSTA
                                     (CD_PERGUNTA, TX_RESPOSTA, DT_RESPOSTA, CD_ACIDENTE, CD_USUARIO_RESPONDEU)
                                 VALUES
                                     (:CdPergunta, :TxResposta, :DtResposta, :CdAcidente, :CdUsuarioRespondeu)";

                    var parametros = new DynamicParameters(resposta);
                    await con.ExecuteAsync(query, parametros);
                    transacao.Commit();
                    inseriu = true;
                }
                catch (OracleException ex)
                {
                    transacao.Rollback();
                    throw new Exception($"Erro ao inserir resposta. Detalhes: {ex.Message}", ex);
                }
            }
            return inseriu;
        }
        #endregion

        #endregion
    }
}
