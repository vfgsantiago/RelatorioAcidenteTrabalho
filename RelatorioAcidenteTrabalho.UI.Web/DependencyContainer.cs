using RelatorioAcidenteTrabalho.Data;
using RelatorioAcidenteTrabalho.Repository;

namespace RelatorioAcidenteTrabalho.UI.Web
{
    public class DependencyContainer
    {
        public static void RegisterContainers(IServiceCollection services)
        {
            #region Site
            services.AddScoped<TipoAcidenteREP>();
            services.AddScoped<PerguntaREP>();
            services.AddScoped<FormularioREP>();
            services.AddScoped<AcidenteREP>();
            services.AddScoped<AcidentadoREP>();
            services.AddScoped<RespostaREP>();
            #endregion

            #region Login
            services.AddScoped<LoginREP>();
            services.AddScoped<UsuarioREP>();
            services.AddScoped<SistemaREP>();
            services.AddScoped<CentroCustoREP>();
            services.AddScoped<FuncaoREP>();
            #endregion

            #region Services
            services.AddScoped<HttpClient>();
            services.AddScoped<AcessaDados>();
            #endregion
        }
    }
}
