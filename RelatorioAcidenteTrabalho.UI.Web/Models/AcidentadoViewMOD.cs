using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidentadoViewMOD
    {
        public IEnumerable<AcidentadoMOD> Lista { get; set; } = new List<AcidentadoMOD>();
        public Paginacao Paginacao { get; set; } = new Paginacao();
    }
}
