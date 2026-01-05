using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidenteViewMOD
    {
        public IEnumerable<AcidenteMOD> Lista { get; set; } = new List<AcidenteMOD>();
        public Paginacao Paginacao { get; set; } = new Paginacao();
    }
}
