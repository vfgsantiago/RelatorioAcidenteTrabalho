using Mapster;
using System.Reflection;
using RelatorioAcidenteTrabalho.Model;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Helpers
{
    public class MappingConfig
    {
        public static void RegisterMaps(IServiceCollection services)
        {
            #region Objects

            #region TipoAcidente
            TypeAdapterConfig<TipoAcidenteMOD, TipoAcidenteViewMOD>
            .NewConfig()
            .Map(member => member.CdTipoAcidente, source => source.CdTipoAcidente)
            .Map(member => member.TxTipoAcidente, source => source.TxTipoAcidente)
            .Map(member => member.TxDescricao, source => source.TxDescricao)
            .Map(member => member.SnAtivo, source => source.SnAtivo)
            .Map(member => member.CdUsuarioCadastrou, source => source.CdUsuarioCadastrou)
            .Map(member => member.DtCadastro, source => source.DtCadastro)
            .Map(member => member.CdUsuarioAlterou, source => source.CdUsuarioAlterou)
            .Map(member => member.DtAlteracao, source => source.DtAlteracao)
            .TwoWays()
            .IgnoreNonMapped(true);
            #endregion

            #region Pergunta
            TypeAdapterConfig<PerguntaMOD, PerguntaViewMOD>
            .NewConfig()
            .Map(member => member.CdPergunta, source => source.CdPergunta)
            .Map(member => member.TxPergunta, source => source.TxPergunta)
            .Map(member => member.SnAtivo, source => source.SnAtivo)
            .Map(member => member.CdUsuarioCadastrou, source => source.CdUsuarioCadastrou)
            .Map(member => member.DtCadastro, source => source.DtCadastro)
            .Map(member => member.CdUsuarioAlterou, source => source.CdUsuarioAlterou)
            .Map(member => member.DtAlteracao, source => source.DtAlteracao)
            .TwoWays()
            .IgnoreNonMapped(true);
            #endregion

            #endregion

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
