using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidentadoDetalheViewMOD
    {
        public AcidentadoMOD Acidentado { get; set; }
        public List<AcidenteMOD> ListaAcidentes { get; set; } = new();
    }
}
