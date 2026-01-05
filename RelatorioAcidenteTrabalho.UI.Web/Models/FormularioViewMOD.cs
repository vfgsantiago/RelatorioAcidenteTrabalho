using RelatorioAcidenteTrabalho.Model;

namespace RelatorioAcidenteTrabalho.UI.Web.Models
{
    public class FormularioViewMOD
    {
        public TipoAcidenteMOD TipoAcidente { get; set; }
        public List<PerguntaMOD> PerguntasVinculadas { get; set; }
        public List<PerguntaMOD> PerguntasDisponiveis { get; set; }
    }
}
