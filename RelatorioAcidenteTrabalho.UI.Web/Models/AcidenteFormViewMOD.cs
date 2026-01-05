using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class AcidenteFormViewMOD
    {
        public TipoAcidenteMOD TipoAcidente { get; set; }
        public List<PerguntaTipoAcidenteMOD> ListaPergunta { get; set; }
    }
}
