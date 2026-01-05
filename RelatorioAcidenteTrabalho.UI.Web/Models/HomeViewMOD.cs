using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class HomeViewMOD
    {
        public List<TipoAcidenteMOD> ListaTipoAcidente { get; set; }
        public int QtdTotalAcidentes { get; set; }
        public int QtdTotalAcidentados { get; set; }
        public int QtdTotalCategorias { get; set; }
    }
}
