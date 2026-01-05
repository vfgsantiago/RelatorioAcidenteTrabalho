using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class AcidentadoREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public AcidentadoREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorCodigo
        public async Task<AcidentadoMOD> BuscarPorCodigo(int cdAcidentado)
        {
            using var con = new OracleConnection(_conexaoOracle);
            var query = @"SELECT A.CD_ACIDENTADO,
                                       A.CD_USUARIO,
                                       A.NR_MATRICULA,
                                       A.TX_NOME,
                                       A.CD_CARGO,
                                       C.NMCARGO AS TxCargo,
                                       A.CD_CENTRO_CUSTO,
                                       CC.NOCENTROCUSTO AS TxCentroCusto,
                                       A.CD_FUNCAO,
                                       F.NMFUNCAO AS TxFuncao,
                                       A.CD_UNIDADE,
                                       U.NOUNIDADE AS TxUnidade,
                                       A.TX_TELEFONE
                                  FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                  WHERE A.CD_CARGO = C.CDCARGO(+)
                                    AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                    AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                    AND A.CD_UNIDADE = U.CDUNIDADE(+)
                                    AND A.CD_ACIDENTADO = :CdAcidentado";

            var parametros = new DynamicParameters();
            parametros.Add("CdAcidentado", cdAcidentado, DbType.Int32);

            var acidentado = await con.QueryFirstOrDefaultAsync<AcidentadoMOD>(query, parametros);
            return acidentado;
        }
        #endregion

        #region BuscarPorMatricula
        public async Task<AcidentadoMOD> BuscarPorMatricula(int nrMatricula)
        {
            using var con = new OracleConnection(_conexaoOracle);
            var query = @"SELECT A.CD_ACIDENTADO,
                                       A.CD_USUARIO,
                                       A.NR_MATRICULA,
                                       A.TX_NOME,
                                       A.CD_CARGO,
                                       C.NMCARGO AS TxCargo,
                                       A.CD_CENTRO_CUSTO,
                                       CC.NOCENTROCUSTO AS TxCentroCusto,
                                       A.CD_FUNCAO,
                                       F.NMFUNCAO AS TxFuncao,
                                       A.CD_UNIDADE,
                                       U.NOUNIDADE AS TxUnidade,
                                       A.TX_TELEFONE
                                  FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                  WHERE A.CD_CARGO = C.CDCARGO(+)
                                    AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                    AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                    AND A.CD_UNIDADE = U.CDUNIDADE(+)
                                    AND A.NR_MATRICULA = :NrMatricula";

            var parametros = new DynamicParameters();
            parametros.Add("NrMatricula", nrMatricula, DbType.Int32);

            var acidentado = await con.QueryFirstOrDefaultAsync<AcidentadoMOD>(query, parametros);
            return acidentado;
        }
        #endregion

        #region Buscar
        public async Task<IEnumerable<AcidenteMOD>> Buscar()
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                string query = @"SELECT A.CD_ACIDENTADO,
                                       A.CD_USUARIO,
                                       A.NR_MATRICULA,
                                       A.TX_NOME,
                                       A.CD_CARGO,
                                       C.NMCARGO AS TxCargo,
                                       A.CD_CENTRO_CUSTO,
                                       CC.NOCENTROCUSTO AS TxCentroCusto,
                                       A.CD_FUNCAO,
                                       F.NMFUNCAO AS TxFuncao,
                                       A.CD_UNIDADE,
                                       U.NOUNIDADE AS TxUnidade,
                                       A.TX_TELEFONE
                                  FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                  WHERE A.CD_CARGO = C.CDCARGO(+)
                                    AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                    AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                    AND A.CD_UNIDADE = U.CDUNIDADE(+)";

                return await con.QueryAsync<AcidenteMOD>(query);
            }
        }
        #endregion

        #region BuscarPorNomeOuMatricula
        /// <summary>
        /// Busca os usuários por nome ou matrícula
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<List<AcidentadoMOD>> BuscarPorNomeOuMatricula(string? filtro)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                var parametros = new DynamicParameters();
                string condicaoFiltro = "";
                if (!string.IsNullOrWhiteSpace(filtro) && filtro.Trim().Length >= 3)
                {
                    filtro = filtro.Trim().ToUpper();
                    parametros.Add("Filtro", $"%{filtro}%");

                    condicaoFiltro = @" AND (
                                              UPPER(A.TX_NOME) LIKE UPPER(:Filtro)
                                           OR A.NR_MATRICULA     LIKE :Filtro
                                           )";
                }

                string query = $@"SELECT A.CD_ACIDENTADO,
                                         A.CD_USUARIO,
                                         A.NR_MATRICULA,
                                         A.TX_NOME,
                                         A.CD_CARGO,
                                         C.NMCARGO AS TxCargo,
                                         A.CD_CENTRO_CUSTO,
                                         CC.NOCENTROCUSTO AS TxCentroCusto,
                                         A.CD_FUNCAO,
                                         F.NMFUNCAO AS TxFuncao,
                                         A.CD_UNIDADE,
                                         U.NOUNIDADE AS TxUnidade,
                                         A.TX_TELEFONE
                                    FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                    WHERE A.CD_CARGO = C.CDCARGO(+)
                                      AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                      AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                      AND A.CD_UNIDADE = U.CDUNIDADE(+)
                                         {condicaoFiltro}
                                     ORDER BY A.TX_NOME ASC";

                var lista = await con.QueryAsync<AcidentadoMOD>(query, parametros);
                return lista.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar acidentados por nome ou matrícula.", ex);
            }
        }
        #endregion

        #region BuscarPaginadoComFiltro
        /// <summary>
        /// Busca os acidentados de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada de acidentados</returns>
        public async Task<PaginacaoResposta<AcidentadoMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, int? cdAcidentado, int? cdCentroCusto, int? cdFuncao)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                int offset = (pagina - 1) * itensPorPagina;
                var parametros = new DynamicParameters();
                string filtros = "";
                if (cdAcidentado.HasValue)
                {
                    filtros += " AND A.CD_ACIDENTADO = :CdAcidentado ";
                    parametros.Add("CdAcidentado", cdAcidentado.Value);
                }
                if (cdCentroCusto.HasValue)
                {
                    filtros += " AND A.CD_CENTRO_CUSTO = :CdCentroCusto ";
                    parametros.Add("CdCentroCusto", cdCentroCusto.Value);
                }
                if (cdFuncao.HasValue)
                {
                    filtros += " AND A.CD_FUNCAO = :CdFuncao ";
                    parametros.Add("CdFuncao", cdFuncao.Value);
                }
                parametros.Add("Offset", offset);
                parametros.Add("ItensPorPagina", itensPorPagina);

                string query = @$"SELECT A.CD_ACIDENTADO,
                                         A.CD_USUARIO,
                                         A.NR_MATRICULA,
                                         A.TX_NOME,
                                         A.CD_CARGO,
                                         C.NMCARGO AS TxCargo,
                                         A.CD_CENTRO_CUSTO,
                                         CC.NOCENTROCUSTO AS TxCentroCusto,
                                         A.CD_FUNCAO,
                                         F.NMFUNCAO AS TxFuncao,
                                         A.CD_UNIDADE,
                                         U.NOUNIDADE AS TxUnidade,
                                         A.TX_TELEFONE,
                                         (
                                             SELECT COUNT(1)
                                               FROM RAT_ACIDENTE RA
                                              WHERE RA.CD_ACIDENTADO = A.CD_ACIDENTADO
                                         ) AS QtdAcidente
                                    FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                    WHERE A.CD_CARGO = C.CDCARGO(+)
                                      AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                      AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                      AND A.CD_UNIDADE = U.CDUNIDADE(+)
                                       {filtros}
                                     ORDER BY A.TX_NOME ASC
                                     OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<AcidentadoMOD>(query, parametros)).ToList();

                string countQuery = @$"SELECT COUNT(*)
                                         FROM RAT_ACIDENTADO A, CARGO C, CENTRO_CUSTO CC, FUNCAO F, UNIDADE U
                                        WHERE A.CD_CARGO = C.CDCARGO(+)
                                          AND A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO(+)
                                          AND A.CD_FUNCAO = F.CDFUNCAO(+)
                                          AND A.CD_UNIDADE = U.CDUNIDADE(+)
                                         {filtros}";
                int totalItens = await con.ExecuteScalarAsync<int>(countQuery, parametros);

                return new PaginacaoResposta<AcidentadoMOD>
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
                throw new Exception("Erro ao buscar acidentados paginados.", ex);
            }
        }
        #endregion

        #region Inserir
        public async Task<int> Inserir(AcidentadoMOD acidentado)
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                var query = @"INSERT INTO RAT_ACIDENTADO
                                          (CD_USUARIO, NR_MATRICULA, TX_NOME, CD_FUNCAO, CD_UNIDADE, CD_CARGO, CD_CENTRO_CUSTO, TX_TELEFONE)
                                     VALUES
                                          (:CdUsuario, :NrMatricula, :TxNome, :CdFuncao, :CdUnidade, :CdCargo, :CdCentroCusto, :TxTelefone)
                                     RETURNING CD_ACIDENTADO INTO :cdAcidentado";

                var parameters = new DynamicParameters();
                parameters.Add(":CdUsuario", acidentado.CdUsuario, dbType: DbType.Int32);
                parameters.Add("NrMatricula", acidentado.NrMatricula, dbType: DbType.Int32);
                parameters.Add("TxNome", acidentado.TxNome, dbType: DbType.String);
                parameters.Add("CdCargo", acidentado.CdCargo ?? 0, dbType: DbType.Int32);
                parameters.Add("CdCentroCusto", acidentado.CdCentroCusto ?? 0, dbType: DbType.Int32);
                parameters.Add("CdFuncao", acidentado.CdFuncao ?? 0, dbType: DbType.Int32);
                parameters.Add("CdUnidade", acidentado.CdUnidade ?? 0, dbType: DbType.Int32);
                parameters.Add("TxTelefone", acidentado.TxTelefone, dbType: DbType.String);
                parameters.Add("cdAcidentado", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(query, parameters);
                return parameters.Get<int>("cdAcidentado");
            }
        }
        #endregion

        #region AtualizarContato
        public bool AtualizarContato(AcidentadoMOD acidentado)
        {
            bool atualizou = false;
            using (OracleConnection con = new OracleConnection(_conexaoOracle))
            {
                con.Open();
                OracleTransaction transacao = con.BeginTransaction();
                try
                {
                    string query = @"UPDATE RAT_ACIDENTADO
                                        SET TX_TELEFONE = :TxTelefone
                                      WHERE CD_ACIDENTADO = :CdAcidentado";

                    var parametros = new DynamicParameters(acidentado);

                    parametros.Add("TxTelefone", acidentado.TxTelefone);
                    parametros.Add("CdAcidentado", acidentado.CdAcidentado);
                    con.Execute(query, parametros);
                    transacao.Commit();
                    atualizou = true;
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                }
            }
            return atualizou;
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
                                           FROM RAT_ACIDENTADO
                                          WHERE 1=1";
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
