using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.Repository
{
    public class FuncaoREP
    {
        #region Conections
        private readonly IConfiguration _configuration;
        private readonly AcessaDados _acessaDados;
        private readonly string _conexaoOracle;
        #endregion

        #region Constructor
        public FuncaoREP(IConfiguration configuration, HttpClient httpClient, AcessaDados acessaDados)
        {
            _configuration = configuration;
            _acessaDados = acessaDados;
            _conexaoOracle = _acessaDados.conexaoOracle();
        }
        #endregion

        #region Methods

        #region Buscar
        /// <summary>
        /// Busca todos as funções ativos
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<FuncaoMOD>> Buscar()
        {
            List<FuncaoMOD> lista = new List<FuncaoMOD>();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT F.CDFUNCAO,
                                                F.CDCARGO,
                                                C.NMCARGO,
                                                F.AOATIVO,
                                                F.NMFUNCAO
                                         FROM FUNCAO F, CARGO C
                                        WHERE F.CDCARGO = C.CDCARGO
                                          AND F.AOATIVO = 'S'
                                         ORDER BY F.CDFUNCAO";
                    lista = (await con.QueryAsync<FuncaoMOD>(query)).ToList();
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
        /// Busca a função o pelo código
        /// </summary>
        /// <param name="cdFuncao"></param>
        /// <returns></returns>
        public async Task<FuncaoMOD> BuscarPorCodigo(int cdFuncao)
        {
            FuncaoMOD model = new FuncaoMOD();
            using (var con = new OracleConnection(_conexaoOracle))
            {
                try
                {
                    con.Open();
                    var query = @"SELECT F.CDFUNCAO,
                                                F.CDCARGO,
                                                C.NMCARGO,
                                                F.AOATIVO,
                                                F.NMFUNCAO
                                         FROM FUNCAO F, CARGO C
                                        WHERE F.CDCARGO = C.CDCARGO
                                          AND F.AOATIVO = 'S'
                                          AND F.CDFUNCAO = :cdFuncao";
                    model = await con.QueryFirstOrDefaultAsync<FuncaoMOD>(query, new { cdFuncao });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return model;
        }
        #endregion

        #region BuscarPorNomeOuCargo
        /// <summary>
        /// Busca a função pelo nome ou pelo cargo
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<List<FuncaoMOD>> BuscarPorNomeOuCargo(string? filtro)
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
                                              UPPER(F.NMFUNCAO) LIKE UPPER(:Filtro)
                                           OR C.NMCARGO LIKE UPPER(:Filtro)
                                           )";
                }

                string query = $@"SELECT F.CDFUNCAO,
                                         F.CDCARGO,
                                         C.NMCARGO,
                                         F.AOATIVO,
                                         F.NMFUNCAO
                                    FROM FUNCAO F,
                                         CARGO  C
                                   WHERE F.CDCARGO = C.CDCARGO
                                     AND F.AOATIVO = 'S'
                                     AND EXISTS (
                                           SELECT 1
                                             FROM RAT_ACIDENTADO A
                                            WHERE A.CD_FUNCAO = F.CDFUNCAO
                                              AND A.CD_CARGO  = F.CDCARGO
                                         )
                                         {condicaoFiltro}
                                     ORDER BY F.NMFUNCAO ASC";
                var lista = await con.QueryAsync<FuncaoMOD>(query, parametros);
                return lista.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao buscar função ou cargo.", ex);
            }
        }
        #endregion

        #endregion
    }
}
