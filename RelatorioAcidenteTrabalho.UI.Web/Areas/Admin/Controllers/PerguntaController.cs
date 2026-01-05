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
    public class PerguntaController : Controller
    {
        #region Repositories
        private readonly PerguntaREP _repositorioPergunta;
        #endregion

        #region Constructor
        public PerguntaController(PerguntaREP repositorioPergunta)
        {
            _repositorioPergunta = repositorioPergunta;
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

            var resultado = await _repositorioPergunta.BuscarPaginadoComFiltro(numeroPagina, _take, filtro);

            var perguntaViewMOD = new PerguntaViewMOD
            {
                ListaPergunta = resultado.Dados,
                QtdTotalDeRegistros = resultado.Paginacao.TotalItens,
                PaginaAtual = resultado.Paginacao.PaginaAtual,
                TotalPaginas = resultado.Paginacao.TotalPaginas
            };

            ViewBag.Filtro = filtro;
            ViewBag.Titulo = "Pergunta";
            return View("Index", perguntaViewMOD);
        }
        #endregion

        #region Cadastrar
        public IActionResult Cadastrar()
        {
            PerguntaMOD perguntaMOD = new PerguntaMOD();
            var perguntaViewMOD = perguntaMOD.Adapt<PerguntaViewMOD>();
            return View(perguntaViewMOD);
        }

        [HttpPost]
        public IActionResult Cadastrar(PerguntaViewMOD dadosTela)
        {
            var perguntaMOD = dadosTela.Adapt<PerguntaMOD>();
            perguntaMOD.TxPergunta = dadosTela.TxPergunta;
            perguntaMOD.SnAtivo = "S";
            perguntaMOD.CdUsuarioCadastrou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            perguntaMOD.DtCadastro = DateTime.Now;
            perguntaMOD.DtAlteracao = DateTime.Now;
            var cadastrou = _repositorioPergunta.Cadastrar(perguntaMOD);
            if (cadastrou)
            {
                TempData["Modal-Sucesso"] = "Pergunta cadastrada com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao cadastrar pergunta!";
                return View(dadosTela);
            }
        }
        #endregion

        #region Editar
        public async Task<IActionResult> Editar(int cdPergunta)
        {
            var perguntaMOD = await _repositorioPergunta.BuscarPorCodigo(cdPergunta);
            if (perguntaMOD == null)
            {
                TempData["Modal-Erro"] = "Pergunta não encontrada!";
                return RedirectToAction("Index");
            }
            var perguntaViewMOD = perguntaMOD.Adapt<PerguntaViewMOD>();
            return View(perguntaViewMOD);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(PerguntaViewMOD dadosTela)
        {
            var perguntaMOD = dadosTela.Adapt<PerguntaMOD>();
            perguntaMOD.TxPergunta = dadosTela.TxPergunta;
            perguntaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            perguntaMOD.DtAlteracao = DateTime.Now;
            perguntaMOD.CdPergunta = dadosTela.CdPergunta;
            var editou = _repositorioPergunta.Editar(perguntaMOD);
            if (editou)
            {
                TempData["Modal-Sucesso"] = "Pergunta alterada com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Modal-Erro"] = "Erro ao alterar pergunta!";
                return View(dadosTela);
            }
        }
        #endregion

        #region AlterarStatus
        [HttpPost]
        public async Task<IActionResult> AlterarStatus(int cdPergunta)
        {
            var perguntaMOD = await _repositorioPergunta.BuscarPorCodigo(cdPergunta);
            if (perguntaMOD == null)
            {
                TempData["Modal-Erro"] = "Pergunta não encontrada!";
                return RedirectToAction("Index");
            }
            perguntaMOD.SnAtivo = perguntaMOD.SnAtivo == "S" ? "N" : "S";
            perguntaMOD.CdUsuarioAlterou = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Contains("CdUsuario"))?.Value);
            perguntaMOD.DtAlteracao = DateTime.Now;
            var alterouStatus = _repositorioPergunta.AlterarStatus(perguntaMOD);
            if (alterouStatus)
                TempData["Modal-Sucesso"] = $"Pergunta {(perguntaMOD.SnAtivo == "S" ? "ativado" : "desativado")} com sucesso!";
            else
                TempData["Modal-Erro"] = "Erro ao alterar status da pergunta. Tente novamente.";

            return RedirectToAction("Index");
        }
        #endregion

        #endregion

    }
}
