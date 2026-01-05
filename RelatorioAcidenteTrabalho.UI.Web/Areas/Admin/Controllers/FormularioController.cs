using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class FormularioController : Controller
    {
        #region Repositories
        private readonly TipoAcidenteREP _repositorioTipoAcidente;
        private readonly PerguntaREP _repositorioPergunta;
        private readonly FormularioREP _repositorioFormulario;
        #endregion

        #region Constructor
        public FormularioController(TipoAcidenteREP repositorioTipoAcidente, PerguntaREP repositorioPergunta, FormularioREP repositorioFormulario)
        {
            _repositorioTipoAcidente = repositorioTipoAcidente;
            _repositorioPergunta = repositorioPergunta;
            _repositorioFormulario = repositorioFormulario;
        }
        #endregion

        #region Parameters
        private const int _take = 15;
        private int _numeroPagina = 1;
        private int _pagina;
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index()
        {
            var tipoAcidente = await _repositorioTipoAcidente.Buscar();
            return View(tipoAcidente);
        }
        #endregion

        #region Hub
        public async Task<IActionResult> Hub(string t)
        {
            if(t == null)
            {
                return RedirectToAction("Index", "Formulario");
            }

            int cdTipoAcidente = Convert.ToInt32(FuncoesParaProgramar.FuncaoCriptografia.Descriptografar(t));

            var tipoAcidente = await _repositorioTipoAcidente.BuscarPorCodigo(cdTipoAcidente);
            var vinculadas = await _repositorioFormulario.BuscarPerguntasPorTipoAcidente(cdTipoAcidente);
            var disponiveis = await _repositorioFormulario.BuscarDisponiveis(cdTipoAcidente);

            var viewMOD = new FormularioViewMOD
            {
                TipoAcidente = tipoAcidente,
                PerguntasVinculadas = vinculadas,
                PerguntasDisponiveis = disponiveis
            };
            return View(viewMOD);
        }
        #endregion

        #region Vincular
        [HttpPost]
        public async Task<IActionResult> Vincular(int cdTipoAcidente, int cdPergunta)
        {
            try
            {
                await _repositorioFormulario.Vincular(cdTipoAcidente, cdPergunta);
                return Ok(new { message = "Pergunta vinculada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao vincular a pergunta.", detail = ex.Message });
            }
        }
        #endregion

        #region Remover
        [HttpPost]
        public async Task<IActionResult> Remover(int cdTipoAcidente, int cdPergunta)
        {
            try
            {
                await _repositorioFormulario.RemoverVinculo(cdTipoAcidente, cdPergunta);
                return Ok(new { message = "Pergunta removida com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao remover a pergunta.", detail = ex.Message });
            }
        }
        #endregion

        #region AtualizarOrdem
        public async Task<IActionResult> AtualizarOrdem([FromBody] AtualizarOrdemViewMOD model)
        {
            if (model == null || model.ordem == null)
                return BadRequest(new { message = "Dados inválidos." });

            try
            {
                await _repositorioFormulario.AtualizarOrdem(model.cdTipoAcidente, model.ordem);
                return Ok(new { message = "Ordem atualizada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao atualizar a ordem.", detail = ex.Message });
            }
        }
        #endregion

        #endregion
    }
}
