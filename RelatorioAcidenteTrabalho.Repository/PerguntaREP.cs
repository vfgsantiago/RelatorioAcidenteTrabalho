using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class PerguntaREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public PerguntaREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorCodigo
        /// <summary>
        /// Busca a pergunta por código
        /// </summary>
        /// <param name="cdPergunta"></param>
        /// <returns></returns>
        public async Task<PerguntaMOD> BuscarPorCodigo(int cdPergunta)
        {
            PerguntaMOD model = new PerguntaMOD();
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
                                              U.NOUSUARIO AS NoUsuarioAlterou,
                                              P.DT_ALTERACAO
                                          FROM RAT_PERGUNTA P, USUARIO U
                                         WHERE P.CD_USUARIO_ALTEROU = U.CDUSUARIO(+) 
                                           AND CD_PERGUNTA = :cdPergunta";
                    model = await con.QueryFirstOrDefaultAsync<PerguntaMOD>(query, new { cdPergunta });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #region BuscarPaginadoComFiltro
        /// <summary>
        /// Busca as perguntas de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada das pergunas</returns>
        public async Task<PaginacaoResposta<PerguntaMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, string? filtro)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                int offset = (pagina - 1) * itensPorPagina;
                var parametros = new DynamicParameters();
                parametros.Add("Offset", offset);
                parametros.Add("ItensPorPagina", itensPorPagina);
                string condicaoFiltro = "";
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    filtro = filtro.Trim().ToUpper();
                    parametros.Add("Filtro", $"%{filtro}%");

                    condicaoFiltro += @"AND   (
                                                  UPPER(P.TX_PERGUNTA) LIKE :Filtro
                                                OR
                                                  P.CD_PERGUNTA LIKE :Filtro
                                                )";
                }
                var query = $@"SELECT P.CD_PERGUNTA,
                                              P.TX_PERGUNTA,
                                              P.SN_ATIVO,
                                              P.CD_USUARIO_CADASTROU,
                                              P.DT_CADASTRO,
                                              P.CD_USUARIO_ALTEROU,
                                              U.NOUSUARIO AS NoUsuarioAlterou,
                                              P.DT_ALTERACAO
                                           FROM RAT_PERGUNTA P, USUARIO U
                                         WHERE P.CD_USUARIO_ALTEROU = U.CDUSUARIO(+)        
                                          {condicaoFiltro}
                                          ORDER BY P.CD_PERGUNTA DESC
                                          OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<PerguntaMOD>(query, parametros)).ToList();

                var totalQuery = $@"SELECT
                                              COUNT(*)
                                          FROM 
                                              RAT_PERGUNTA P
                                         WHERE 1=1                             
                                          {condicaoFiltro}";
                int totalItens = await con.ExecuteScalarAsync<int>(totalQuery, parametros);

                return new PaginacaoResposta<PerguntaMOD>
                {
                    Dados = lista,
                    Paginacao = new Paginacao
                    {
                        PaginaAtual = pagina,
                        QuantidadePorPagina = itensPorPagina,
                        TotalItens = totalItens,
                        TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina)
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar as perguntas paginadas com filtro.", ex);
            }
        }
        #endregion

        #region Cadastrar
        /// <summary>
        /// Cadastrar a pergunta
        /// </summary>
        /// <param name="perguntaMOD"></param>
        /// <returns></returns>
        public bool Cadastrar(PerguntaMOD perguntaMOD)
        {
            bool cadastrou = false;

            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();

                try
                {
                    string query = @"INSERT INTO RAT_PERGUNTA
                                                 (
                                                  TX_PERGUNTA,
                                                  SN_ATIVO,
                                                  CD_USUARIO_CADASTROU,
                                                  DT_CADASTRO,
                                                  DT_ALTERACAO
                                                 )
                                           VALUES
                                                 (
                                                 :TxPergunta,
                                                 :SnAtivo,
                                                 :CdUsuarioCadastrou,
                                                 :DtCadastro,
                                                 :DtAlteracao
                                                 )";

                    var parametros = new DynamicParameters(perguntaMOD);

                    parametros.Add("TxPergunta", perguntaMOD.TxPergunta);
                    parametros.Add("SnAtivo", perguntaMOD.SnAtivo);
                    parametros.Add("CdUsuarioCadastrou", perguntaMOD.CdUsuarioCadastrou);
                    parametros.Add("DtCadastro", perguntaMOD.DtCadastro);
                    parametros.Add("DtAlteracao", perguntaMOD.DtAlteracao);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    cadastrou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return cadastrou;
        }
        #endregion

        #region Editar
        /// <summary>
        /// Editar a pergunta
        /// </summary>
        /// <param name="perguntaMOD"></param>
        /// <returns></returns>
        public bool Editar(PerguntaMOD perguntaMOD)
        {
            bool editou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE RAT_PERGUNTA
                                        SET
                                            TX_PERGUNTA = :TxPergunta,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO = :DtAlteracao
                                      WHERE
                                            CD_PERGUNTA = :CdPergunta";

                    var parametros = new DynamicParameters(perguntaMOD);

                    parametros.Add("TxPergunta", perguntaMOD.TxPergunta);
                    parametros.Add("CdUsuarioAlterou", perguntaMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", perguntaMOD.DtAlteracao);
                    parametros.Add("CdPergunta", perguntaMOD.CdPergunta);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    editou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return editou;
        }
        #endregion

        #region AlterarStatus
        /// <summary>
        /// Altera o status da perguna
        /// </summary>
        /// <param name="perguntaMOD"></param>
        /// <returns></returns>
        public bool AlterarStatus(PerguntaMOD perguntaMOD)
        {
            bool alterouStatus = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE RAT_PERGUNTA
                                        SET
                                            SN_ATIVO = :SnAtivo,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO = :DtAlteracao
                                      WHERE
                                            CD_PERGUNTA = :CdPergunta";

                    var parametros = new DynamicParameters(perguntaMOD);

                    parametros.Add("SnAtivo", perguntaMOD.SnAtivo);
                    parametros.Add("CdUsuarioAlterou", perguntaMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", perguntaMOD.DtAlteracao);
                    parametros.Add("CdPergunta", perguntaMOD.CdPergunta);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    alterouStatus = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return alterouStatus;
        }
        #endregion

        #endregion
    }
}
