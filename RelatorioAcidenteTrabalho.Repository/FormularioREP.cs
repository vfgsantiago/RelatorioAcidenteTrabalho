using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class FormularioREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public FormularioREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPerguntasPorTipoAcidente
        /// <summary>
        /// Busca todos as perguntas por tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <returns></returns>
        public async Task<List<PerguntaMOD>> BuscarPerguntasPorTipoAcidente(int cdTipoAcidente)
        {
            List<PerguntaMOD> lista = new List<PerguntaMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT P.CD_PERGUNTA,
                                              P.TX_PERGUNTA,
                                              P.SN_ATIVO,
                                              P.CD_USUARIO_CADASTROU,
                                              P.DT_CADASTRO,
                                              P.CD_USUARIO_ALTEROU,
                                              P.DT_ALTERACAO
                                          FROM RAT_PERGUNTA P, RAT_PERGUNTA_ACIDENTE_TIPO PAT
                                         WHERE P.CD_PERGUNTA = PAT.CD_PERGUNTA
                                           AND PAT.CD_TIPO_ACIDENTE = :CdTipoAcidente
                                           AND P.SN_ATIVO = 'S'
                                        ORDER BY PAT.NR_ORDEM_EXIBICAO";
                    lista = (await con.QueryAsync<PerguntaMOD>(query, new {CdTipoAcidente = cdTipoAcidente})).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarFormularioPorTipoAcidente
        /// <summary>
        /// Busca o formulário por tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <returns></returns>
        public async Task<List<PerguntaTipoAcidenteMOD>> BuscarFormularioPorTipoAcidente(int cdTipoAcidente)
        {
            List<PerguntaTipoAcidenteMOD> lista = new List<PerguntaTipoAcidenteMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT PAT.CD_PERGUNTA_TIPO_ACIDENTE,
                                                PAT.CD_TIPO_ACIDENTE,
                                                T.TX_TIPO_ACIDENTE,
                                                PAT.CD_PERGUNTA,
                                                P.TX_PERGUNTA,
                                                PAT.NR_ORDEM_EXIBICAO
                                           FROM RAT_PERGUNTA_ACIDENTE_TIPO PAT, RAT_PERGUNTA P, RAT_ACIDENTE_TIPO T
                                          WHERE PAT.CD_PERGUNTA = P.CD_PERGUNTA
                                            AND PAT.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE
                                            AND PAT.CD_TIPO_ACIDENTE = :CdTipoAcidente
                                            AND P.SN_ATIVO = 'S'
                                          ORDER BY PAT.NR_ORDEM_EXIBICAO";
                    lista = (await con.QueryAsync<PerguntaTipoAcidenteMOD>(query, new { CdTipoAcidente = cdTipoAcidente })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarPerguntaPorCodigo
        /// <summary>
        /// Busca a pergunta pelo código
        /// </summary>
        /// <param name="cdPerguntaTipoAcidente"></param>
        /// <returns></returns>
        public async Task<PerguntaMOD> BuscarPerguntaPorCodigo(int cdPerguntaTipoAcidente)
        {
            PerguntaMOD pergunta = new PerguntaMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT P.CD_PERGUNTA,
                                              P.TX_PERGUNTA,
                                              P.SN_ATIVO,
                                              P.CD_USUARIO_CADASTROU,
                                              P.DT_CADASTRO,
                                              P.CD_USUARIO_ALTEROU,
                                              P.DT_ALTERACAO
                                          FROM RAT_PERGUNTA P, RAT_PERGUNTA_ACIDENTE_TIPO PAT
                                         WHERE P.CD_PERGUNTA = PAT.CD_PERGUNTA
                                           AND PAT.CD_PERGUNTA_TIPO_ACIDENTE = :cdPerguntaTipoAcidente
                                           AND P.SN_ATIVO = 'S'";
                    pergunta = await con.QueryFirstOrDefaultAsync<PerguntaMOD>(query, new { CdPerguntaTipoAcidente = cdPerguntaTipoAcidente });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return pergunta;
        }
        #endregion

        #region Vincular
        /// <summary>
        /// Vincular perguntas ao tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <param name="cdPergunta"></param>
        /// <returns></returns>
        public async Task Vincular(int cdTipoAcidente, int cdPergunta)
        {
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                await con.OpenAsync();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO RAT_PERGUNTA_ACIDENTE_TIPO
                                                 (
                                                  CD_TIPO_ACIDENTE,
                                                  CD_PERGUNTA,
                                                  NR_ORDEM_EXIBICAO
                                                 )
                                           VALUES
                                                 (
                                                 :CdTipoAcidente,
                                                 :CdPergunta,
                                                  COALESCE((SELECT MAX(NR_ORDEM_EXIBICAO)+1 
                                                            FROM RAT_PERGUNTA_ACIDENTE_TIPO 
                                                            WHERE CD_TIPO_ACIDENTE = :cdTipoAcidente), 1))";

                    await con.ExecuteAsync(query, new { CdTipoAcidente = cdTipoAcidente, CdPergunta = cdPergunta }, transacao);
                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
        }
        #endregion

        #region RemoverVinculo
        /// <summary>
        /// Remover vínculo de pergunta e tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <param name="cdPergunta"></param>
        /// <returns></returns>
        public async Task RemoverVinculo(int cdTipoAcidente, int cdPergunta)
        {
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                await con.OpenAsync();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM RAT_PERGUNTA_ACIDENTE_TIPO
                                           WHERE CD_TIPO_ACIDENTE = :CdTipoAcidente
                                             AND CD_PERGUNTA = :CdPergunta";
                    await con.ExecuteAsync(query, new { CdTipoAcidente = cdTipoAcidente, CdPergunta = cdPergunta }, transacao);
                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
        }
        #endregion

        #region BuscarDisponiveis
        /// <summary>
        /// Busca todos as perguntas não vinculadas ao tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <returns></returns>
        public async Task<List<PerguntaMOD>> BuscarDisponiveis(int cdTipoAcidente)
        {
            List<PerguntaMOD> lista = new List<PerguntaMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT P.CD_PERGUNTA,
                                              P.TX_PERGUNTA,
                                              P.SN_ATIVO,
                                              P.CD_USUARIO_CADASTROU,
                                              P.DT_CADASTRO,
                                              P.CD_USUARIO_ALTEROU,
                                              P.DT_ALTERACAO
                                          FROM RAT_PERGUNTA P
                                         WHERE P.SN_ATIVO = 'S'
                                           AND P.CD_PERGUNTA NOT IN
                                                  (SELECT PAT.CD_PERGUNTA
                                                      FROM RAT_PERGUNTA_ACIDENTE_TIPO PAT
                                                     WHERE PAT.CD_TIPO_ACIDENTE = :CdTipoAcidente)";
                    lista = (await con.QueryAsync<PerguntaMOD>(query, new { CdTipoAcidente = cdTipoAcidente })).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region AtualizarOrdem
        /// <summary>
        /// Atualizar a ordem de exibição das perguntas vinculadas ao tipo de acidente
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <param name="ordem"></param>
        /// <returns></returns>
        public async Task AtualizarOrdem(int cdTipoAcidente, List<int> ordem)
        {
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                await con.OpenAsync();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    for(int i=0; i<ordem.Count; i++)
                    {
                        string query = @"UPDATE RAT_PERGUNTA_ACIDENTE_TIPO
                                           SET NR_ORDEM_EXIBICAO = :Ordem
                                         WHERE CD_TIPO_ACIDENTE = :CdTipoAcidente
                                           AND CD_PERGUNTA = :CdPergunta";
                        await con.ExecuteAsync(query, new { Ordem = i + 1, CdTipoAcidente = cdTipoAcidente, CdPergunta = ordem[i] }, transacao);
                    }
                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
        }
        #endregion

        #endregion
    }
}
