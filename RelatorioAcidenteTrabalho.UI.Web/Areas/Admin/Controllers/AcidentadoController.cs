using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AcidentadoController : Controller
    {
        #region Repositories
        private readonly AcidentadoREP _repositorioAcidentado;
        private readonly AcidenteREP _repositorioAcidente;
        private readonly CentroCustoREP _repositorioCentroCusto;
        private readonly FuncaoREP _repositorioFuncao;
        #endregion

        #region Parameters
        private const int _take = 15;
        private int _numeroPagina = 1;
        private int _pagina;
        #endregion

        #region Constructor
        public AcidentadoController(
           AcidentadoREP repositorioAcidentado,
           AcidenteREP repositorioAcidente,
           CentroCustoREP repositorioCentroCusto,
           FuncaoREP repositorioFuncao)
        {
            _repositorioAcidentado = repositorioAcidentado;
            _repositorioAcidente = repositorioAcidente;
            _repositorioCentroCusto = repositorioCentroCusto;
            _repositorioFuncao = repositorioFuncao;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(int? pagina, int? cdAcidentado, int? cdCentroCusto, int? cdFuncao)
        {
            int numeroPagina = pagina ?? 1;

            ViewBag.ListaAcidentados = await _repositorioAcidentado.Buscar();
            if (cdAcidentado.HasValue)
            {
                var acidentado = await _repositorioAcidentado.BuscarPorCodigo(cdAcidentado.Value);
                if (acidentado != null)
                    ViewBag.AcidentadoNome = $"{acidentado.TxNome} ({acidentado.NrMatricula})";
            }

            ViewBag.ListaCentroCustoUnidade = await _repositorioCentroCusto.Buscar();
            if (cdCentroCusto.HasValue)
            {
                var centroCusto = await _repositorioCentroCusto.BuscarPorCodigo(cdCentroCusto.Value);
                if (centroCusto != null)
                    ViewBag.CentroCustoUnidadeNome = $"{centroCusto.NoCentroCusto}";
            }

            ViewBag.ListaFuncaoCargo = await _repositorioFuncao.Buscar();
            if (cdFuncao.HasValue)
            {
                var funcao = await _repositorioFuncao.BuscarPorCodigo(cdFuncao.Value);
                if (funcao != null)
                    ViewBag.FuncaoCargoNome = $"{funcao.NmFuncao}";
            }

            var resultado = await _repositorioAcidentado.BuscarPaginadoComFiltro(
                numeroPagina, _take, cdAcidentado, cdCentroCusto, cdFuncao);

            var viewMOD = new AcidentadoViewMOD
            {
                Lista = resultado.Dados,
                Paginacao = resultado.Paginacao
            };

            ViewBag.Acidentado = cdAcidentado;
            ViewBag.CentroCustoUnidade = cdCentroCusto;
            ViewBag.FuncaoCargo = cdFuncao;

            return View(viewMOD);
        }

        #endregion

        #region Detalhe
        public async Task<IActionResult> Detalhe(string u)
        {
            if(u == null)
            {
                TempData["Modal-Erro"] = "Falha ao buscar acidentado, tente novamente mais tarde!";
                return RedirectToAction("Index", "Acidentado", new {area = "Admin" });
            }

            int cdAcidentado = Convert.ToInt32(FuncoesParaProgramar.FuncaoCriptografia.Descriptografar(u));

            var acidentado = await _repositorioAcidentado.BuscarPorCodigo(cdAcidentado);
            var listaAcidentes = await _repositorioAcidente.BuscarPorAcidentado(cdAcidentado);

            var viewMOD = new AcidentadoDetalheViewMOD
            {
                Acidentado = acidentado,
                ListaAcidentes = listaAcidentes
            };
            return View(viewMOD);
        }
        #endregion

        #region Helpers

        #region BuscarAcidentados
        [HttpGet]
        public async Task<IActionResult> BuscarAcidentados(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo) || termo.Length < 3)
                return Json(new { sucesso = true, acidentados = new List<object>() });

            var lista = await _repositorioAcidentado.BuscarPorNomeOuMatricula(termo);

            var resultado = lista.Select(x => new
            {
                x.CdAcidentado,
                x.TxNome,
                x.NrMatricula
            });

            return Json(new { sucesso = true, acidentados = resultado });
        }
        #endregion

        #region BuscarCentroCustoUnidade
        [HttpGet]
        public async Task<IActionResult> BuscarCentroCustoUnidade(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo) || termo.Length < 3)
                return Json(new { sucesso = true, centroCusto = new List<object>() });

            var lista = await _repositorioCentroCusto.BuscarPorNomeOuUnidade(termo);

            var resultado = lista.Select(x => new
            {
                x.CdCentroCusto,
                x.NoCentroCusto,
                x.NoUnidade
            });

            return Json(new { sucesso = true, centroCusto = resultado });
        }
        #endregion

        #region BucarFuncaoCargo
        [HttpGet]
        public async Task<IActionResult> BuscarFuncaoCargo(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo) || termo.Length < 3)
                return Json(new { sucesso = true, funcao = new List<object>() });

            var lista = await _repositorioFuncao.BuscarPorNomeOuCargo(termo);

            var resultado = lista.Select(x => new
            {
                x.CdFuncao,
                x.NmFuncao,
                x.NmCargo
            });

            return Json(new { sucesso = true, funcao = resultado });
        }
        #endregion

        #endregion

        #endregion
    }
}
