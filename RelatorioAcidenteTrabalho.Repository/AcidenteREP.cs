using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class AcidenteREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public AcidenteREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region BuscarPorCodigo
        public async Task<AcidenteMOD> BuscarPorCodigo(int cdAcidente)
        {
            using var con = new OracleConnection(_conexaoOracle);
            var query = @"SELECT A.CD_ACIDENTE,
                                       A.CD_TIPO_ACIDENTE,
                                       T.TX_TIPO_ACIDENTE,
                                       A.TX_TITULO,
                                       A.TX_OBSERVACAO,
                                       A.DT_ACIDENTE,
                                       A.CD_ACIDENTADO,
                                       A.DT_REGISTRO,
                                       A.CD_USUARIO_REGISTROU,
                                       U.NOUSUARIO AS TxUsuarioRegistrou
                                  FROM RAT_ACIDENTE A, RAT_ACIDENTE_TIPO T, USUARIO U
                                 WHERE A.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE
                                   AND A.CD_USUARIO_REGISTROU = U.CDUSUARIO
                                   AND A.CD_ACIDENTE = :CdAcidente";
            var parametros = new DynamicParameters();
            parametros.Add("CdAcidente", cdAcidente, DbType.Int32);
            var acidente = await con.QueryFirstOrDefaultAsync<AcidenteMOD>(query, parametros);
            return acidente;
        }
        #endregion

        #region BuscarUltimosNove
        public async Task<IEnumerable<AcidenteMOD>> BuscarUltimosNove()
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                string query = @"SELECT A.CD_ACIDENTE,
                                        A.CD_TIPO_ACIDENTE,
                                        T.TX_TIPO_ACIDENTE,
                                        A.TX_TITULO,
                                        A.TX_OBSERVACAO,
                                        A.CD_ACIDENTADO,
                                        AC.TX_NOME,
                                        AC.NR_MATRICULA,
                                        A.DT_ACIDENTE,
                                        A.CD_USUARIO_REGISTROU,
                                        A.DT_REGISTRO
                                   FROM RAT_ACIDENTE A, RAT_ACIDENTE_TIPO T, RAT_ACIDENTADO AC
                                  WHERE A.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE     
                                    AND A.CD_ACIDENTADO = AC.CD_ACIDENTADO                                
                                  ORDER BY DT_REGISTRO DESC
                                  FETCH FIRST 9 ROWS ONLY";

                return await con.QueryAsync<AcidenteMOD>(query);
            }
        }
        #endregion

        #region BuscarPaginadoComFiltro
        /// <summary>
        /// Busca os acidentes de forma paginada, e com filtros
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="itensPorPagina"></param>
        /// <returns>Lista paginada de acidentes</returns>
        public async Task<PaginacaoResposta<AcidenteMOD>> BuscarPaginadoComFiltro(int pagina, int itensPorPagina, int? cdAcidente, int? cdTipoAcidente, int? cdAcidentado, DateTime? dtInicioPeriodo, DateTime? dtFimPeriodo)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                int offset = (pagina - 1) * itensPorPagina;
                var parametros = new DynamicParameters();
                string filtros = "";
                if (cdAcidente.HasValue)
                {
                    filtros += " AND A.CD_ACIDENTE = :CdAcidente ";
                    parametros.Add("CdAcidente", cdAcidente.Value);
                }
                if (cdTipoAcidente.HasValue)
                {
                    filtros += " AND A.CD_TIPO_ACIDENTE = :CdTipoAcidente ";
                    parametros.Add("CdTipoAcidente", cdTipoAcidente.Value);
                }
                if (cdAcidentado.HasValue)
                {
                    filtros += " AND A.CD_ACIDENTADO = :CdAcidentado ";
                    parametros.Add("CdAcidentado", cdAcidentado.Value);
                }
                if (dtInicioPeriodo.HasValue)
                {
                    filtros += " AND A.DT_ACIDENTE >= :DtInicio ";
                    parametros.Add("DtInicio", dtInicioPeriodo.Value);
                }
                if (dtFimPeriodo.HasValue)
                {
                    filtros += " AND A.DT_ACIDENTE <= :DtFim ";
                    parametros.Add("DtFim", dtFimPeriodo.Value);
                }
                parametros.Add("Offset", offset);
                parametros.Add("ItensPorPagina", itensPorPagina);

                string query = @$"SELECT A.CD_ACIDENTE,
                                         A.TX_TITULO,
                                         A.TX_OBSERVACAO,
                                         A.DT_ACIDENTE,
                                         A.CD_TIPO_ACIDENTE,
                                         T.TX_TIPO_ACIDENTE,
                                         A.CD_ACIDENTADO,
                                         AC.TX_NOME,
                                         AC.NR_MATRICULA,
                                         A.CD_USUARIO_REGISTROU,
                                         U.NOUSUARIO AS TxUsuarioRegistrou,
                                         A.DT_REGISTRO
                                     FROM RAT_ACIDENTE A,
                                          RAT_ACIDENTE_TIPO T,
                                          RAT_ACIDENTADO AC,
                                          USUARIO U
                                     WHERE A.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE
                                       AND A.CD_ACIDENTADO = AC.CD_ACIDENTADO
                                       AND A.CD_USUARIO_REGISTROU = U.CDUSUARIO
                                       {filtros}
                                     ORDER BY A.DT_REGISTRO DESC
                                     OFFSET :Offset ROWS FETCH NEXT :ItensPorPagina ROWS ONLY";
                var lista = (await con.QueryAsync<AcidenteMOD>(query, parametros)).ToList();

                string countQuery = @$"SELECT COUNT(*)
                                         FROM RAT_ACIDENTE A
                                         WHERE 1 = 1
                                         {filtros}";
                int totalItens = await con.ExecuteScalarAsync<int>(countQuery, parametros);

                return new PaginacaoResposta<AcidenteMOD>
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
                throw new Exception("Erro ao buscar acidentes paginados.", ex);
            }
        }
        #endregion

        #region BuscarPorTitulo
        /// <summary>
        /// Busca os acidente por titulo
        /// </summary>
        /// <param name="titulo"></param>
        /// <returns></returns>
        public async Task<List<AcidenteMOD>> BuscarPorTitulo(string? titulo)
        {
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                var parametros = new DynamicParameters();
                string condicaoFiltro = "";
                if (!string.IsNullOrWhiteSpace(titulo) && titulo.Trim().Length >= 3)
                {
                    titulo = titulo.Trim().ToUpper();
                    parametros.Add("Filtro", $"%{titulo}%");

                    condicaoFiltro = @" AND UPPER(A.TX_TITULO) LIKE UPPER(:Filtro)";
                }

                string query = $@"SELECT A.CD_ACIDENTE,
                                         A.CD_TIPO_ACIDENTE,
                                         T.TX_TIPO_ACIDENTE,
                                         A.TX_TITULO,
                                         A.TX_OBSERVACAO,
                                         A.DT_ACIDENTE,
                                         A.CD_ACIDENTADO,
                                         A.DT_REGISTRO,
                                         A.CD_USUARIO_REGISTROU,
                                         U.NOUSUARIO AS TxUsuarioRegistrou
                                    FROM RAT_ACIDENTE A, RAT_ACIDENTE_TIPO T, USUARIO U
                                   WHERE A.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE
                                     AND A.CD_USUARIO_REGISTROU = U.CDUSUARIO
                                         {condicaoFiltro}
                                     ORDER BY A.TX_TITULO ASC";
                var lista = await con.QueryAsync<AcidenteMOD>(query, parametros);
                return lista.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar acidente por nome ou matrícula.", ex);
            }
        }
        #endregion

        #region BuscarPorAcidentado
        /// <summary>
        /// Busca os acidente por acidentado
        /// </summary>
        /// <param name="cdAcidentado"></param>
        /// <returns></returns>
        public async Task<List<AcidenteMOD>> BuscarPorAcidentado(int cdAcidentado)
        {
            List<AcidenteMOD> lista = new List<AcidenteMOD>();
            using var con = new OracleConnection(_conexaoOracle);
            try
            {
                await con.OpenAsync();
                string query = $@"SELECT A.CD_ACIDENTE,
                                         A.CD_TIPO_ACIDENTE,
                                         T.TX_TIPO_ACIDENTE,
                                         A.TX_TITULO,
                                         A.TX_OBSERVACAO,
                                         A.DT_ACIDENTE,
                                         A.CD_ACIDENTADO,
                                         A.DT_REGISTRO,
                                         A.CD_USUARIO_REGISTROU,
                                         U.NOUSUARIO AS TxUsuarioRegistrou
                                    FROM RAT_ACIDENTE A, RAT_ACIDENTE_TIPO T, USUARIO U
                                   WHERE A.CD_TIPO_ACIDENTE = T.CD_TIPO_ACIDENTE
                                     AND A.CD_USUARIO_REGISTROU = U.CDUSUARIO
                                     AND A.CD_ACIDENTADO = :CdAcidentado
                                     ORDER BY A.TX_TITULO ASC";
                var parametros = new DynamicParameters();
                parametros.Add("CdAcidentado", cdAcidentado, DbType.Int32);
                lista = (List<AcidenteMOD>)await con.QueryAsync<AcidenteMOD>(query, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar acidente por nome ou matrícula.", ex);
            }
            return lista.ToList();
        }
        #endregion

        #region Inserir
        public async Task<int> Inserir(AcidenteMOD acidente)
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                string query = @"INSERT INTO RAT_ACIDENTE
                                       (CD_TIPO_ACIDENTE, TX_TITULO, TX_OBSERVACAO,
                                       CD_ACIDENTADO, DT_ACIDENTE, CD_USUARIO_REGISTROU, DT_REGISTRO)
                                  VALUES
                                      (:CdTipoAcidente, :TxTitulo, :TxObservacao,
                                       :CdAcidentado, :DtAcidente, :CdUsuarioRegistrou, :DtRegistro)
                                  RETURNING CD_ACIDENTE INTO :cdAcidente";

                var p = new DynamicParameters();
                p.Add(":CdTipoAcidente", acidente.CdTipoAcidente);
                p.Add(":TxTitulo", acidente.TxTitulo);
                p.Add(":TxObservacao", acidente.TxObservacao);
                p.Add(":CdAcidentado", acidente.CdAcidentado);
                p.Add(":DtAcidente", acidente.DtAcidente);
                p.Add(":CdUsuarioRegistrou", acidente.CdUsuarioRegistrou);
                p.Add(":DtRegistro", acidente.DtRegistro);
                p.Add(":cdAcidente", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(query, p);
                return p.Get<int>("cdAcidente");
            }
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
                                           FROM RAT_ACIDENTE
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

        #region ContarHoje
        public async Task<int> ContaHoje()
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                string query = @"SELECT COUNT(*)
                                   FROM RAT_ACIDENTE
                                  WHERE TRUNC(DT_ACIDENTE) = TRUNC(SYSDATE)";

                return await con.ExecuteScalarAsync<int>(query);
            }
        }
        #endregion

        #region MediaDiaria
        public async Task<decimal> MediaDiaria()
        {
            using (var con = new OracleConnection(_conexaoOracle))
            {
                string query = @"SELECT ROUND(
                                         (SELECT COUNT(*) 
                                            FROM RAT_ACIDENTE
                                           WHERE DT_ACIDENTE >= TRUNC(SYSDATE, 'MM')
                                             AND DT_ACIDENTE < ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1)
                                         )
                                         /
                                         TO_NUMBER(TO_CHAR(SYSDATE, 'DD')),
                                       2) AS MEDIA
                                FROM DUAL";

                var result = await con.ExecuteScalarAsync<object>(query);
                if (result == null || result == DBNull.Value)
                    return 0;
                return Convert.ToDecimal(result);
            }
        }
        #endregion

        #endregion
    }
}
