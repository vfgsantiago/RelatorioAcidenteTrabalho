using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        #region Repositories
        private readonly AcidenteREP _repositorioAcidente;
        private readonly AcidentadoREP _repositorioAcidentado;
        #endregion

        #region Constructors
        public HomeController(
            AcidenteREP repositorioAcidente,
            AcidentadoREP repositorioAcidentado
            )
        {
            _repositorioAcidente = repositorioAcidente;
            _repositorioAcidentado = repositorioAcidentado;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index()
        {
            HomeAdminViewMOD viewMOD = new HomeAdminViewMOD();
            viewMOD.QtdAcidentadosTotal = await _repositorioAcidentado.ContarAtivos();
            viewMOD.QtdAcidentesTotal = await _repositorioAcidente.ContarAtivos();
            viewMOD.QtdAcidentesHoje = await _repositorioAcidente.ContaHoje();
            viewMOD.QtdMediaDiariaAcidentes = await _repositorioAcidente.MediaDiaria();
            viewMOD.ListaAcidente = await _repositorioAcidente.BuscarUltimosNove();
            return View(viewMOD);
        }
        #endregion

        #endregion
    }
}
