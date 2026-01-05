using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class TipoAcidenteREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public TipoAcidenteREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos os tipos de acidentes
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<TipoAcidenteMOD>> Buscar()
        {
            List<TipoAcidenteMOD> lista = new List<TipoAcidenteMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT
                                              T.CD_TIPO_ACIDENTE,
                                              T.TX_TIPO_ACIDENTE,
                                              T.TX_DESCRICAO,
                                              T.SN_ATIVO,
                                              T.CD_USUARIO_CADASTROU,
                                              T.DT_CADASTRO,
                                              T.CD_USUARIO_ALTEROU,
                                              T.DT_ALTERACAO
                                          FROM RAT_ACIDENTE_TIPO T
                                         WHERE T.SN_ATIVO = 'S'
                                        ORDER BY T.CD_TIPO_ACIDENTE";
                    lista = (await con.QueryAsync<TipoAcidenteMOD>(query)).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarAtivosComPerguntas
        /// <summary>
        /// Busca todos os tipos de acidentes
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<TipoAcidenteMOD>> BuscarAtivosComPerguntas()
        {
            List<TipoAcidenteMOD> lista = new List<TipoAcidenteMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT DISTINCT
                                               T.CD_TIPO_ACIDENTE,
                                               T.TX_TIPO_ACIDENTE,
                                               T.TX_DESCRICAO,
                                               T.SN_ATIVO,
                                               T.CD_USUARIO_CADASTROU,
                                               T.DT_CADASTRO,
                                               T.CD_USUARIO_ALTEROU,
                                               T.DT_ALTERACAO
                                          FROM RAT_ACIDENTE_TIPO T,
                                               RAT_PERGUNTA_ACIDENTE_TIPO PAT,
                                               RAT_PERGUNTA P
                                         WHERE T.SN_ATIVO = 'S'
                                           AND T.CD_TIPO_ACIDENTE = PAT.CD_TIPO_ACIDENTE
                                           AND PAT.CD_PERGUNTA = P.CD_PERGUNTA
                                         ORDER BY T.CD_TIPO_ACIDENTE";
                    lista = (await con.QueryAsync<TipoAcidenteMOD>(query)).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return lista;
        }
        #endregion

        #region BuscarPorCodigo
        /// <summary>
        /// Busca o tipo de acidente por código
        /// </summary>
        /// <param name="cdTipoAcidente"></param>
        /// <returns></returns>
        public async Task<TipoAcidenteMOD> BuscarPorCodigo(int cdTipoAcidente)
        {
            TipoAcidenteMOD model = new TipoAcidenteMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT
                                              T.CD_TIPO_ACIDENTE,
                                              T.TX_TIPO_ACIDENTE,
                                              T.TX_DESCRICAO,
                                              T.SN_ATIVO,
                                              T.CD_USUARIO_CADASTROU,
                                              T.DT_CADASTRO,
                                              T.CD_USUARIO_ALTEROU,
                                              U.NOUSUARIO AS NoUsuarioAlterou,
                                              T.DT_ALTERACAO
                                          FROM RAT_ACIDENTE_TIPO T, USUARIO U
                                         WHERE T.CD_USUARIO_ALTEROU = U.CDUSUARIO(+) 
                                           AND CD_TIPO_ACIDENTE = :cdTipoAcidente";
                    model = await con.QueryFirstOrDefaultAsync<TipoAcidenteMOD>(query, new { cdTipoAcidente });
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
        /// Busca os tipos de acidentes de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada de tipos de acidentes</returns>
        public async Task<PaginacaoResposta<TipoAcidenteMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, string? filtro)
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
                                                  UPPER(T.TX_TIPO_ACIDENTE) LIKE :Filtro
                                                OR
                                                  T.CD_TIPO_ACIDENTE LIKE :Filtro
                                                )";
                }
                var query = $@"SELECT
                                              T.TX_TIPO_ACIDENTE,
                                              T.TX_DESCRICAO,
                                              T.CD_TIPO_ACIDENTE,
                                              T.SN_ATIVO,
                                              T.CD_USUARIO_CADASTROU,
                                              T.DT_CADASTRO,
                                              T.CD_USUARIO_ALTEROU,
                                              U.NOUSUARIO AS NoUsuarioAlterou,
                                              T.DT_ALTERACAO
                                          FROM RAT_ACIDENTE_TIPO T, USUARIO U
                                         WHERE T.CD_USUARIO_ALTEROU = U.CDUSUARIO(+)          
                                          {condicaoFiltro}
                                          ORDER BY T.CD_TIPO_ACIDENTE DESC
                                          OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<TipoAcidenteMOD>(query, parametros)).ToList();

                var totalQuery = $@"SELECT
                                              COUNT(*)
                                          FROM 
                                              RAT_ACIDENTE_TIPO T
                                         WHERE 1=1                             
                                          {condicaoFiltro}";
                int totalItens = await con.ExecuteScalarAsync<int>(totalQuery, parametros);

                return new PaginacaoResposta<TipoAcidenteMOD>
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
                throw new Exception("Erro ao buscar tipos de acidente paginado com filtro.", ex);
            }
        }
        #endregion

        #region Cadastrar
        /// <summary>
        /// Cadastrar o tipo de acidente
        /// </summary>
        /// <param name="tipoAcidenteMOD"></param>
        /// <returns></returns>
        public bool Cadastrar(TipoAcidenteMOD tipoAcidenteMOD)
        {
            bool cadastrou = false;

            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();

                try
                {
                    string query = @"INSERT INTO RAT_ACIDENTE_TIPO
                                                 (
                                                  TX_TIPO_ACIDENTE,
                                                  TX_DESCRICAO,
                                                  SN_ATIVO,
                                                  CD_USUARIO_CADASTROU,
                                                  DT_CADASTRO,
                                                  DT_ALTERACAO
                                                 )
                                           VALUES
                                                 (
                                                 :TxTipoAcidente,
                                                 :TxDescricao,
                                                 :SnAtivo,
                                                 :CdUsuarioCadastrou,
                                                 :DtCadastro,
                                                 :DtAlteracao
                                                 )";

                    var parametros = new DynamicParameters(tipoAcidenteMOD);

                    parametros.Add("TxTipoAcidente", tipoAcidenteMOD.TxTipoAcidente);
                    parametros.Add("TxDescricao", tipoAcidenteMOD.TxDescricao);
                    parametros.Add("SnAtivo", tipoAcidenteMOD.SnAtivo);
                    parametros.Add("CdUsuarioCadastrou", tipoAcidenteMOD.CdUsuarioCadastrou);
                    parametros.Add("DtCadastro", tipoAcidenteMOD.DtCadastro);
                    parametros.Add("DtAlteracao", tipoAcidenteMOD.DtAlteracao);
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
        /// Editar o tipo de acidente
        /// </summary>
        /// <param name="tipoAcidenteMOD"></param>
        /// <returns></returns>
        public bool Editar(TipoAcidenteMOD tipoAcidenteMOD)
        {
            bool editou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE RAT_ACIDENTE_TIPO
                                        SET
                                            TX_TIPO_ACIDENTE = :TxTipoAcidente,
                                            TX_DESCRICAO = :TxDescricao,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO = :DtAlteracao
                                      WHERE
                                            CD_TIPO_ACIDENTE = :CdTipoAcidente";

                    var parametros = new DynamicParameters(tipoAcidenteMOD);

                    parametros.Add("TxDescricao", tipoAcidenteMOD.TxDescricao);
                    parametros.Add("CdUsuarioAlterou", tipoAcidenteMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", tipoAcidenteMOD.DtAlteracao);
                    parametros.Add("CdTipoAcidente", tipoAcidenteMOD.CdTipoAcidente);
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
        /// Altera o status do tipo de acidente
        /// </summary>
        /// <param name="tipoAcidenteMOD"></param>
        /// <returns></returns>
        public bool AlterarStatus(TipoAcidenteMOD tipoAcidenteMOD)
        {
            bool alterouStatus = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE RAT_ACIDENTE_TIPO
                                        SET
                                            SN_ATIVO = :SnAtivo,
                                            CD_USUARIO_ALTEROU = :CdUsuarioAlterou,
                                            DT_ALTERACAO = :DtAlteracao
                                      WHERE
                                            CD_TIPO_ACIDENTE = :CdTipoAcidente";

                    var parametros = new DynamicParameters(tipoAcidenteMOD);

                    parametros.Add("SnAtivo", tipoAcidenteMOD.SnAtivo);
                    parametros.Add("CdUsuarioAlterou", tipoAcidenteMOD.CdUsuarioAlterou);
                    parametros.Add("DtAlteracao", tipoAcidenteMOD.DtAlteracao);
                    parametros.Add("CdTipoAcidente", tipoAcidenteMOD.CdTipoAcidente);
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

        #region Contar
        /// <summary>
        /// Conta todos os tipos de acidentes ativos
        /// </summary>
        /// <returns>Total de registros ativos</returns>
        public async Task<int> ContarAtivos()
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT COUNT(*)
                                           FROM RAT_ACIDENTE_TIPO
                                          WHERE SN_ATIVO = 'S'";
                    return await con.ExecuteScalarAsync<int>(query);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #endregion
    }
}