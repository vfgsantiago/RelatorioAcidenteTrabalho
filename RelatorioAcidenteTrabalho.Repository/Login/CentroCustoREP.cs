using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class CentroCustoREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public CentroCustoREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos os centro de custo ativos
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<CentroCustoMOD>> Buscar()
        {
            List<CentroCustoMOD> lista = new List<CentroCustoMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT CC.CDCENTROCUSTO,
                                               CC.CDUNIDADE,
                                               U.NOUNIDADE,
                                               CC.NOCENTROCUSTO,
                                               CC.AOATIVO
                                          FROM CENTRO_CUSTO CC, UNIDADE U
                                         WHERE CC.CDUNIDADE = U.CDUNIDADE 
                                           AND CC.AOATIVO = 'S'
                                         ORDER BY CC.CDCENTROCUSTO";
                    lista = (await con.QueryAsync<CentroCustoMOD>(query)).ToList();
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
        /// Busca o  centro de custo pelo código
        /// </summary>
        /// <param name="cdCentroCusto"></param>
        /// <returns></returns>
        public async Task<CentroCustoMOD> BuscarPorCodigo(int cdCentroCusto)
        {
            CentroCustoMOD model = new CentroCustoMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT CC.CDCENTROCUSTO,
                                               CC.CDUNIDADE,
                                               U.NOUNIDADE,
                                               CC.NOCENTROCUSTO,
                                               CC.AOATIVO
                                          FROM CENTRO_CUSTO CC, UNIDADE U
                                         WHERE CC.CDUNIDADE = U.CDUNIDADE 
                                           AND CC.AOATIVO = 'S'
                                           AND CC.CDCENTROCUSTO = :cdCentroCusto";
                    model = await con.QueryFirstOrDefaultAsync<CentroCustoMOD>(query, new { cdCentroCusto });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #region BuscarPorNomeOuUnidade
        /// <summary>
        /// Busca o centro de custo pelo nome ou pela unidade
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<List<CentroCustoMOD>> BuscarPorNomeOuUnidade(string? filtro)
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
                                              UPPER(CC.NOCENTROCUSTO) LIKE UPPER(:Filtro)
                                           OR U.NOUNIDADE LIKE UPPER(:Filtro)
                                           )";
                }

                string query = $@"SELECT CC.CDCENTROCUSTO,
                                         CC.CDUNIDADE,
                                         U.NOUNIDADE,
                                         CC.NOCENTROCUSTO,
                                         CC.AOATIVO
                                    FROM CENTRO_CUSTO CC,
                                         UNIDADE      U
                                   WHERE CC.CDUNIDADE = U.CDUNIDADE
                                     AND CC.AOATIVO   = 'S'
                                     AND EXISTS (
                                           SELECT 1
                                             FROM RAT_ACIDENTADO A
                                            WHERE A.CD_CENTRO_CUSTO = CC.CDCENTROCUSTO
                                              AND A.CD_UNIDADE      = CC.CDUNIDADE
                                         )
                                       {condicaoFiltro}
                                     ORDER BY CC.NOCENTROCUSTO ASC";
                var lista = await con.QueryAsync<CentroCustoMOD>(query, parametros);
                return lista.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar centro de custo ou unidade.", ex);
            }
        }
        #endregion

        #endregion
    }
}
