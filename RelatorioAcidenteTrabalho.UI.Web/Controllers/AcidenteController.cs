using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatorioAcidenteTrabalho.Model;
using RelatorioAcidenteTrabalho.Repository;
using RelatorioAcidenteTrabalho.UI.Web.Models;
using RelatorioAcidenteTrabalho.UI.Web.Services;

namespace RelatorioAcidenteTrabalho.UI.Web.Controllers
{
    [AllowAnonymous]
    public class AcidenteController : Controller
    {
        #region Repositories
        private readonly TipoAcidenteREP _repositorioTipoAcidente;
        private readonly AcidentadoREP _repositorioAcidentado;
        private readonly AcidenteREP _repositorioAcidente;
        private readonly PerguntaREP _repositorioPergunta;
        private readonly FormularioREP _repositorioFormulario;
        private readonly LoginREP _repositorioLogin;
        private readonly RespostaREP _repositorioResposta;
        private readonly UsuarioREP _repositorioUsuario;
        private readonly IEmailService _emailService;
        private readonly ILogger<AcidenteController> _logger;
        #endregion

        #region Parameters
        private readonly string _emailSetor;
        #endregion

        #region Constructor
        public AcidenteController(
            TipoAcidenteREP repositorioTipoAcidente,
            AcidentadoREP repositorioAcidentado,
            AcidenteREP repositorioAcidente,
            PerguntaREP repositorioPergunta,
            FormularioREP repositorioFormulario,
            LoginREP repositorioLogin,
            RespostaREP repositorioResposta,
            UsuarioREP repositorioUsuario,
            IEmailService emailService,
            ILogger<AcidenteController> logger,
            IConfiguration configuration)
        {
            _repositorioTipoAcidente = repositorioTipoAcidente;
            _repositorioAcidentado = repositorioAcidentado;
            _repositorioAcidente = repositorioAcidente;
            _repositorioPergunta = repositorioPergunta;
            _repositorioFormulario = repositorioFormulario;
            _repositorioLogin = repositorioLogin;
            _repositorioResposta = repositorioResposta;
            _repositorioUsuario = repositorioUsuario;
            _emailService = emailService;
            _logger = logger;
            _emailSetor = configuration.GetSection("EmailSetor").Value;
        }
        #endregion

        #region Methods

        #region Index
        public async Task<IActionResult> Index(string t)
        {
            if(t == null)
            {
                return RedirectToAction("Index", "Home");
            }

            int cdTipoAcidente = Convert.ToInt32(FuncoesParaProgramar.FuncaoCriptografia.Descriptografar(t));

            var tipo = await _repositorioTipoAcidente.BuscarPorCodigo(cdTipoAcidente);
            if (tipo == null)
            {
                TempData["Modal-Erro"] = "Categoria de acidente inválida.";
                return RedirectToAction("Index", "Home");
            }

            var perguntas = await _repositorioFormulario.BuscarFormularioPorTipoAcidente(cdTipoAcidente);
            var view = new AcidenteFormViewMOD
            {
                TipoAcidente = tipo,
                ListaPergunta = perguntas
            };

            return View(view);
        }
        #endregion

        #region ValidarLogin
        [HttpPost]
        public async Task<IActionResult> ValidarLogin(string login, string senha)
        {
            UsuarioMOD usuario = new UsuarioMOD();
            usuario.TxLogin = login.ToUpper();
            usuario.TxSenha = senha;
            bool valido = await _repositorioLogin.ValidarLogin(usuario);
            if (valido)
            {
                usuario = await _repositorioLogin.BuscarPorLogin(usuario);
                usuario = await _repositorioLogin.BuscarPorCpf(usuario);
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Login ou senha inválidos." });
                }

                return Json(new
                {
                    sucesso = true,
                    usuario = new
                    {
                        usuario.CdUsuario,
                        usuario.NmUsuario,
                        usuario.NrMatricula,
                        usuario.TxEmail
                    }
                });
            }
            else
            {
                return Json(new { sucesso = false, mensagem = "Login ou senha inválidos." });
            }
        }
        #endregion

        #region Enviar
        [HttpPost]
        public async Task<IActionResult> Enviar([FromBody] AcidenteEnvioViewMOD model)
        {
            if (model == null)
                return BadRequest("Modelo nulo ou JSON inválido");

            try
            {
                var usuario = await _repositorioUsuario.BuscarPorMatricula(model.NrMatricula);
                if (usuario != null)
                {
                    int cdAcidentado;
                    var acidentadoJaRegistrado = await _repositorioAcidentado.BuscarPorMatricula(model.NrMatricula);
                    if (acidentadoJaRegistrado == null)
                    {
                        var acidentado = new AcidentadoMOD
                        {
                            CdUsuario = usuario.CdUsuario,
                            NrMatricula = model.NrMatricula,
                            TxNome = model.TxNome,
                            CdCargo = usuario.CdCargo,
                            CdCentroCusto = usuario.CdCentroCusto,
                            CdFuncao = usuario.CdFuncao,
                            CdUnidade = usuario.CdUnidade,
                            TxTelefone = model.TxTelefone
                        };
                        cdAcidentado = await _repositorioAcidentado.Inserir(acidentado);
                    }
                    else
                    {
                        cdAcidentado = acidentadoJaRegistrado.CdAcidentado;
                        var atualizouContato = _repositorioAcidentado.AtualizarContato(acidentadoJaRegistrado);
                    }

                    var acidente = new AcidenteMOD
                    {
                        CdTipoAcidente = model.CdTipoAcidente,
                        TxTitulo = model.TxTitulo,
                        TxObservacao = model.TxObservacao,
                        DtAcidente = model.DtAcidente,
                        CdAcidentado = cdAcidentado,
                        DtRegistro = DateTime.Now,
                        CdUsuarioRegistrou = model.CdUsuario
                    };
                    int cdAcidente = await _repositorioAcidente.Inserir(acidente);

                    foreach (RespostaItemViewMOD item in model.ListaResposta)
                    {
                        var perguntaTipoAcidente = await _repositorioFormulario.BuscarPerguntaPorCodigo(item.CdPerguntaTipoAcidente);
                        var resposta = new RespostaMOD
                        {
                            CdPergunta = perguntaTipoAcidente.CdPergunta,
                            TxResposta = item.TxResposta,
                            DtResposta = DateTime.Now,
                            CdAcidente = cdAcidente,
                            CdUsuarioRespondeu = model.CdUsuario
                        };
                        var inseriu = await _repositorioResposta.Inserir(resposta);
                    }

                    var acidentadoCompleto = await _repositorioAcidentado.BuscarPorCodigo(cdAcidentado);
                    var listaResposta = await _repositorioResposta.BuscarPorAcidente(cdAcidente);
                    var listaPerguntasRespostas = listaResposta.Select(r => new PerguntaRespostaMOD
                    {
                        Pergunta = r.TxPergunta,
                        Resposta = r.TxResposta
                    }).ToList();
                    var dadosEmail = new AcidenteSucessoViewMOD
                    {
                        CdAcidente = cdAcidente,
                        TxTitulo = model.TxTitulo,
                        TxObservacao = model.TxObservacao,
                        TipoAcidente = (await _repositorioTipoAcidente.BuscarPorCodigo(acidente.CdTipoAcidente))?.TxTipoAcidente,
                        DtAcidente = model.DtAcidente,
                        TxNome = acidentadoCompleto.TxNome,
                        NrMatricula = acidentadoCompleto.NrMatricula,
                        TxTelefone = acidentadoCompleto.TxTelefone,
                        CentroCusto = acidentadoCompleto.TxCentroCusto,
                        Unidade = acidentadoCompleto.TxUnidade,
                        Cargo = acidentadoCompleto.TxCargo,
                        TxUsuarioRegistrou = (await _repositorioAcidente.BuscarPorCodigo(cdAcidente))?.TxUsuarioRegistrou,
                        DtRegistro = acidente.DtRegistro,
                        ListaPerguntaResposta = listaPerguntasRespostas
                    };
                    await EnviarEmailInterno(dadosEmail);

                    TempData["Modal-Sucesso"] = "Acidente registrado com sucesso.";
                    string cdAcidenteCriptografado = FuncoesParaProgramar.FuncaoCriptografia.Criptografar(cdAcidente.ToString());
                    return Json(new { success = true, a = cdAcidenteCriptografado });
                }
                else
                {
                    return BadRequest("Usuário não encontrado");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Sucesso
        [HttpGet]
        public async Task<IActionResult> Sucesso(string a)
        {
            if (a == null)
            {
                return RedirectToAction("Index", "Home");
            }

            int cdAcidente = Convert.ToInt32(FuncoesParaProgramar.FuncaoCriptografia.Descriptografar(a));

            var acidente = await _repositorioAcidente.BuscarPorCodigo(cdAcidente);
            if (acidente != null)
            {
                var acidentado = await _repositorioAcidentado.BuscarPorCodigo(acidente.CdAcidentado);
                var respostas = await _repositorioResposta.BuscarPorAcidente(cdAcidente);

                var model = new AcidenteSucessoViewMOD
                {
                    CdAcidente = cdAcidente,
                    TxTitulo = acidente.TxTitulo,
                    TxObservacao = acidente.TxObservacao,
                    DtAcidente = acidente.DtAcidente,
                    TipoAcidente = acidente.TxTipoAcidente,
                    TxNome = acidentado.TxNome,
                    NrMatricula = acidentado.NrMatricula,
                    TxTelefone = acidentado.TxTelefone,
                    CentroCusto = acidentado.TxCentroCusto,
                    Unidade = acidentado.TxUnidade,
                    Cargo = acidentado.TxCargo,
                    Funcao = acidentado.TxFuncao,
                    TxUsuarioRegistrou = acidente.TxUsuarioRegistrou,
                    DtRegistro = acidente.DtRegistro,
                    ListaPerguntaResposta = respostas.Select(r => new PerguntaRespostaMOD
                    {
                        Pergunta = r.TxPergunta,
                        Resposta = r.TxResposta
                    }).ToList()
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion

        #region Helpers

        #region BuscarUsuarios
        [HttpGet]
        public async Task<IActionResult> BuscarUsuarios(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo) || termo.Length < 3)
                return Json(new { sucesso = true, usuarios = new List<object>() });

            var usuarios = await _repositorioUsuario.BuscarPorNomeOuMatricula(termo);
            var resultado = usuarios.Select(x => new
            {
                x.CdUsuario,
                x.NmUsuario,
                x.NrMatricula,
                x.CdCargo,
                x.CdCentroCusto,
                x.CdFuncao,
                x.CdUnidade
            });
            return Json(new { sucesso = true, usuarios = resultado });
        }
        #endregion

        #region EnviarEmailInterno
        private async Task<bool> EnviarEmailInterno(AcidenteSucessoViewMOD model)
        {
            try
            {
                var dadosEmail = new Dictionary<string, string>
                {
                    { "Título do Acidente", model.TxTitulo },
                    { "Data do Acidente", model.DtAcidente.ToString("dd/MM/yyyy") },
                    { "Categoria do Acidente", model.TipoAcidente },
                    { "Nome do Acidentado", model.TxNome },
                    { "Matrícula do Acidentado", model.NrMatricula.ToString() },
                    { "Telefone do Acidentado", model.TxTelefone },
                    { "Setor do Acidentado", model.CentroCusto },
                    { "Cargo do Acidentado", model.Cargo },
                    { "Usuário que Registrou o Acidente", model.TxUsuarioRegistrou },
                    { "Data do Registro do Acidente", model.DtRegistro.ToString("dd/MM/yyyy HH:mm") },
                    { "Observação", string.IsNullOrEmpty(model.TxObservacao) ? "-" : model.TxObservacao }
                };

                await _emailService.EnviarEmailComInformacoes(
                    _emailSetor,
                    "Setor de Segurança do Trabalho",
                    "Novo Relatório de Acidente de Trabalho",
                    "Novo Relatório de Acidente de Trabalho Registrado",
                    "Um novo relatório de acidente de trabalho foi registrado no sistema. Seguem os detalhes abaixo:",
                    dadosEmail,
                    model.ListaPerguntaResposta.Select(r => new PerguntaRespostaMOD
                    {
                        Pergunta = r.Pergunta,
                        Resposta = r.Resposta
                    }).ToList()
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao enviar e-mail interno: {ex.Message}");
                return false;
            }
        }
        #endregion

        #endregion

        #endregion
    }
}
