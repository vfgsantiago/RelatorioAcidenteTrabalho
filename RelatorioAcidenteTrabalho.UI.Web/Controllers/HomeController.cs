using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        #region Repositories
        private readonly TipoAcidenteREP _repositorioTipoAcidente;
        private readonly AcidentadoREP _repositorioAcidentado;
        private readonly AcidenteREP _repositorioAcidente;
        #endregion

        #region  Constructor
        public HomeController(TipoAcidenteREP repositorioTipoAcidente, AcidentadoREP repositorioAcidentado, AcidenteREP repositorioAcidente)
        {
            _repositorioTipoAcidente = repositorioTipoAcidente;
            _repositorioAcidentado = repositorioAcidentado;
            _repositorioAcidente = repositorioAcidente;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index()
        {
            HomeViewMOD viewMOD = new HomeViewMOD();
            viewMOD.ListaTipoAcidente = await _repositorioTipoAcidente.BuscarAtivosComPerguntas();
            viewMOD.QtdTotalAcidentes = await _repositorioAcidente.ContarAtivos();
            viewMOD.QtdTotalAcidentados = await _repositorioAcidentado.ContarAtivos();
            viewMOD.QtdTotalCategorias = await _repositorioTipoAcidente.ContarAtivos();
            return View(viewMOD);
        }
        #endregion

        #region Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewMOD { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #endregion
    }
}

