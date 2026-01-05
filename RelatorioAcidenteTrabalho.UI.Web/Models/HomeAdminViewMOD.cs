using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class HomeAdminViewMOD
    {
        public int QtdAcidentadosTotal { get; set; }
        public int QtdAcidentesTotal { get; set; }
        public int QtdAcidentesHoje { get; set; }
        public decimal QtdMediaDiariaAcidentes { get; set; }
        public IEnumerable<AcidenteMOD> ListaAcidente { get; set; }
    }
}
