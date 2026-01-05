using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Model;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TipoAcidenteController : Controller
    {
        #region Repositories
        private readonly TipoAcidenteREP _repositorioTipoAcidente;
        #endregion

        #region Constructor
        public TipoAcidenteController(TipoAcidenteREP repositorioTipoAcidente)
        {
            _repositorioTipoAcidente = repositorioTipoAcidente;
        }
        #endregion

        #region Parameters
        private const int _take = 15;
        private int _numeroPagina = 1;
        private int _pagina;
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(int? pagina, string? filtro)
        {
            int numeroPagina = pagina ?? 1;

            var resultado = await _repositorioTipoAcidente.BuscarPaginadoComFiltro(numeroPagina, _take, filtro);

            var tipoAcidenteViewMOD = new TipoAcidenteViewMOD
            {
                ListaTipoAcidente = resultado.Dados,
                QtdTotalDeRegistros = resultado.Paginacao.TotalItens,
                PaginaAtual = resultado.Paginacao.PaginaAtual,
                TotalPaginas = resultado.Paginacao.TotalPaginas
            };

            ViewBag.Filtro = filtro;
            ViewBag.Titulo = "Tipos de Acidente";
            return View("Index", tipoAcidenteViewMOD);
        }
        #endregion

        #region Cadastrar
        public IActionResult Cadastrar()
        {
            TipoAcidenteMOD tipoAcidenteMOD = new TipoAcidenteMOD();
            var tipoAcidenteViewMOD = tipoAcidenteMOD.Adapt<TipoAcidenteViewMOD>();
            return View(tipoAcidenteViewMOD);
        }

        [HttpPost]
        public IActionResult Cadastrar(TipoAcidenteViewMOD dadosTela)
        {
            var tipoAcidenteMOD = dadosTela.Adapt<TipoAcidenteMOD>();
            tipoAcidenteMOD.TxTipoAcidente = dadosTela.TxTipoAcidente;
            tipoAcidenteMOD.TxDescricao = dadosTela.TxDescricao;
            tipoAcidenteMOD.SnAtivo = "S";
            tipoAcidenteMOD.CdUsuarioCadastrou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            tipoAcidenteMOD.DtCadastro = DateTime.Now;
            tipoAcidenteMOD.DtAlteracao = DateTime.Now;
            var cadastrou = _repositorioTipoAcidente.Cadastrar(tipoAcidenteMOD);
            if (cadastrou)
            {
                TempData["Modal-Sucesso"] = "Categoria de acidente cadastrada com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao cadastrar categoria de acidente!";
                return View(dadosTela);
            }
        }
        #endregion

        #region Editar
        public async Task<IActionResult> Editar(int cdTipoAcidente)
        {
            var tipoAcidenteMOD = await _repositorioTipoAcidente.BuscarPorCodigo(cdTipoAcidente);
            if (tipoAcidenteMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de acidente não encontrada!";
                return RedirectToAction("Index");
            }
            var tipoAcidenteViewMOD = tipoAcidenteMOD.Adapt<TipoAcidenteViewMOD>();
            return View(tipoAcidenteViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoAcidenteViewMOD dadosTela)
        {
            var tipoAcidenteMOD = dadosTela.Adapt<TipoAcidenteMOD>();
            tipoAcidenteMOD.TxTipoAcidente = dadosTela.TxTipoAcidente;
            tipoAcidenteMOD.TxDescricao = dadosTela.TxDescricao;
            tipoAcidenteMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            tipoAcidenteMOD.DtAlteracao = DateTime.Now;
            tipoAcidenteMOD.CdTipoAcidente = dadosTela.CdTipoAcidente;
            var editou = _repositorioTipoAcidente.Editar(tipoAcidenteMOD);
            if (editou)
            {
                TempData["Modal-Sucesso"] = "Categoria de acidente alterada com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao alterar categoria de acidente!";
                return View(dadosTela);
            }
        }
        #endregion

        #region AlterarStatus
        [HttpPost]
        public async Task<IActionResult> AlterarStatus(int cdTipoAcidente)
        {
            var tipoAcidenteMOD = await _repositorioTipoAcidente.BuscarPorCodigo(cdTipoAcidente);
            if (tipoAcidenteMOD == null)
            {
                TempData["Modal-Erro"] = "Categoria de acidente não encontrada!";
                return RedirectToAction("Index");
            }
            tipoAcidenteMOD.SnAtivo = tipoAcidenteMOD.SnAtivo == "S" ? "N" : "S";
            tipoAcidenteMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            tipoAcidenteMOD.DtAlteracao = DateTime.Now;
            var alterouStatus = _repositorioTipoAcidente.AlterarStatus(tipoAcidenteMOD);
            if (alterouStatus)
                TempData["Modal-Sucesso"] = $"Categoria de acidente {(tipoAcidenteMOD.SnAtivo == "S" ? "ativado" : "desativado")} com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar status da categoria de acidente. Tente novamente.";

            return RedirectToAction("Index");
        }
        #endregion

        #endregion
    }
}
