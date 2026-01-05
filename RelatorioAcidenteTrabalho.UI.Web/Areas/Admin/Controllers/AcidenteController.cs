using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AcidenteController : Controller
    {
        #region Repositories
        private readonly TipoAcidenteREP _repositorioTipoAcidente;
        private readonly AcidentadoREP _repositorioAcidentado;
        private readonly AcidenteREP _repositorioAcidente;
        private readonly PerguntaREP _repositorioPergunta;
        private readonly FormularioREP _repositorioFormulario;
        private readonly LoginREP _repositorioLogin;
        private readonly RespostaREP _repositorioResposta;
        private readonly UsuarioREP _repositorioUsuario;
        #endregion

        #region Parameters
        private const int _take = 15;
        private int _numeroPagina = 1;
        private int _pagina;
        #endregion

        #region Constructor
        public AcidenteController(
           TipoAcidenteREP repositorioTipoAcidente,
           AcidentadoREP repositorioAcidentado,
           AcidenteREP repositorioAcidente,
           PerguntaREP repositorioPergunta,
           FormularioREP repositorioFormulario,
           LoginREP repositorioLogin,
           RespostaREP repositorioResposta,
           UsuarioREP repositorioUsuario)
        {
            _repositorioTipoAcidente = repositorioTipoAcidente;
            _repositorioAcidentado = repositorioAcidentado;
            _repositorioAcidente = repositorioAcidente;
            _repositorioPergunta = repositorioPergunta;
            _repositorioFormulario = repositorioFormulario;
            _repositorioLogin = repositorioLogin;
            _repositorioResposta = repositorioResposta;
            _repositorioUsuario = repositorioUsuario;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(int? pagina, int? cdAcidente, int? cdTipoAcidente, int? cdAcidentado, DateTime? dtInicioPeriodo, DateTime? dtFimPeriodo)
        {
            int numeroPagina = pagina ?? 1;

            var listaTipos = await _repositorioTipoAcidente.Buscar();
            ViewBag.ListaTipos = listaTipos;
            var listaAcidentados = await _repositorioAcidentado.Buscar();
            ViewBag.ListaAcidentados = listaAcidentados;

            if(cdAcidente.HasValue)
            {
                var acidente = await _repositorioAcidente.BuscarPorCodigo(cdAcidente.Value);
                if (acidente != null)
                    ViewBag.Titulo = $"{acidente.TxTitulo}";
            }

            if (cdAcidentado.HasValue)
            {
                var acidentado = await _repositorioAcidentado.BuscarPorCodigo(cdAcidentado.Value);
                if (acidentado != null)
                    ViewBag.AcidentadoNome = $"{acidentado.TxNome} ({acidentado.NrMatricula})";
            }

            var resultado = await _repositorioAcidente.BuscarPaginadoComFiltro(
                numeroPagina, _take, cdAcidente, cdTipoAcidente, cdAcidentado, dtInicioPeriodo, dtFimPeriodo);

            var viewMOD = new AcidenteViewMOD
            {
                Lista = resultado.Dados,
                Paginacao = resultado.Paginacao
            };

            ViewBag.Acidente = cdAcidente;
            ViewBag.TipoAcidente = cdTipoAcidente;
            ViewBag.Acidentado = cdAcidentado;
            ViewBag.InicioPeriodo = dtInicioPeriodo;
            ViewBag.FimPeriodo = dtFimPeriodo;

            return View(viewMOD);
        }

        #endregion

        #region Detalhe
        public async Task<IActionResult> Detalhe(string a)
        {
            if (a == null)
            {
                TempData["Modal-Erro"] = "Falha ao buscar acidente, tente novamente mais tarde!";
                return RedirectToAction("Index", "Acidente", new { area = "Admin" });
            }

            int cdAcidente = Convert.ToInt32(FuncoesParaProgramar.FuncaoCriptografia.Descriptografar(a));

            var acidente = await _repositorioAcidente.BuscarPorCodigo(cdAcidente);
            var acidentado = await _repositorioAcidentado.BuscarPorCodigo(acidente.CdAcidentado);
            var respostas = await _repositorioResposta.BuscarPorAcidente(cdAcidente);

            var viewMOD = new AcidenteDetalheViewMOD
            {
                CdAcidente = cdAcidente,
                TxTitulo = acidente.TxTitulo,
                TxObservacao = acidente.TxObservacao,
                DtAcidente = acidente.DtAcidente,
                TxTipoAcidente = acidente.TxTipoAcidente,
                CdAcidentado = acidente.CdAcidentado,
                TxNome = acidentado.TxNome,
                NrMatricula = acidentado.NrMatricula,
                TxTelefone = acidentado.TxTelefone,
                TxCentroCusto = acidentado.TxCentroCusto,
                TxUnidade = acidentado.TxUnidade,
                TxCargo = acidentado.TxCargo,
                TxFuncao = acidentado.TxFuncao,
                TxUsuarioRegistrou = acidente.TxUsuarioRegistrou,
                DtRegistro = acidente.DtRegistro,
                ListaPerguntaResposta = respostas.Select(r => new PerguntaRespostaMOD
                {
                    Pergunta = r.TxPergunta,
                    Resposta = r.TxResposta
                }).ToList()
            };
            return View(viewMOD);
        }
        #endregion

        #region Helpers

        #region BuscarTitulos
        [HttpGet]
        public async Task<IActionResult> BuscarTitulos(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 3)
                return Json(new { sucesso = true, titulos = new List<object>() });

            var lista = await _repositorioAcidente.BuscarPorTitulo(titulo);

            var resultado = lista.Select(x => new
            {
                x.CdAcidente,
                x.TxTitulo
            });

            return Json(new { sucesso = true, titulos = resultado });
        }
        #endregion

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

        #endregion

        #endregion
    }
}
