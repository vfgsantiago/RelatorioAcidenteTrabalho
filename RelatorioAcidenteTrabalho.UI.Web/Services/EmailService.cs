using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using RelatorioAcidenteTrabalho.UI.Web.Helpers;
using RelatorioAcidenteTrabalho.UI.Web.Models;

namespace RelatorioAcidenteTrabalho.UI.Web.Services
{
    public class EmailService : IEmailService
    {
        #region Services
        private EmailSettings _emailSettings { get; }
        private ILogger<EmailService> _logger;
        IConfiguration _configuration;
        #endregion

        #region Constructor
        public EmailService(IOptions<EmailSettings> emailSettings,
      IConfiguration configuration,
      ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
            _logger = logger;
        }
        #endregion

        #region Methods

        #region EnviarEmailComInformacoes
        public async Task EnviarEmailComInformacoes(string email, string nome, string subject, string titulo, string message, Dictionary<string, string> dadosAdicionais, List<PerguntaRespostaMOD> listaPerguntas)
        {
            try
            {
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.FromEmail, "Relatório de Acidente de Trabalho")
                };
                mail.To.Add(new MailAddress(email));
                if (!string.IsNullOrWhiteSpace(_emailSettings.BccEmail))
                    mail.Bcc.Add(new MailAddress(_emailSettings.BccEmail));
                if (!string.IsNullOrWhiteSpace(_emailSettings.CcEmail))
                    mail.CC.Add(new MailAddress(_emailSettings.CcEmail));
                mail.Subject = subject;
                mail.Body = PopulateBody(nome, titulo, message, dadosAdicionais, listaPerguntas);
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Falha ao enviar email - {ex.Message}");
            }
        }
        #endregion

        #region Helpers

        #region PopulateBody
        /// <summary>
                /// Preenche os valores no template padrão
                /// </summary>
                /// <param name="Nome"></param>
                /// <param name="Titulo"></param>
                /// <param name="Texto"></param>
                /// <returns></returns>
        private string PopulateBody(string Nome, string Titulo, string Texto, Dictionary<string, string> dadosAdicionais, List<PerguntaRespostaMOD> listaPerguntas)
        {
            string body = string.Empty;

            string path = "wwwroot/Template/Email.html";

            using (StreamReader reader = new StreamReader(path))
            {
                body = reader.ReadToEnd();
            }

            string infoTableHtml = "";
            if (dadosAdicionais.Count > 0)
            {
                foreach (var item in dadosAdicionais)
                {
                    infoTableHtml += $"<tr><td><strong>{item.Key}:</strong></td><td>{item.Value}</td></tr>";
                }
            }
            body = body.Replace("{LISTA_INFORMACOES}", infoTableHtml);

            string perguntasHtml = "";
            if(listaPerguntas != null && listaPerguntas.Count > 0)
            {
                int rowIndex = 0;
                foreach(var item in listaPerguntas)
                {
                    perguntasHtml += $@"
                                        <tr>
                                            <td style='padding:10px 0;'>
                                                <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                                    <tr>
                                                        <td style='padding:12px 15px; font-weight:bold; color:#411564; font-size:15px;'>
                                                            {item.Pergunta}
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style='padding:12px 15px; color:#333333; font-size:14px; line-height:1.5;'>
                                                            {item.Resposta}
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>";
                    rowIndex++;
                }
            }
            body = body.Replace("{LISTA_PERGUNTAS}", perguntasHtml);

            body = body.Replace("{NOME}", Nome);
            body = body.Replace("{TITULO_EMAIL}", Titulo);
            body = body.Replace("{TEXTO}", Texto);
            body = body.Replace("{ANO_ATUAL}", DateTime.Now.Year.ToString());

            return body;
        }
        #endregion

        #endregion

        #endregion
    }

    public interface IEmailService
    {
        /// <summary>
        /// Envia um email e preenche os parametros no template padrão
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subject"></param>
        /// <param name="titulo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task EnviarEmailComInformacoes(string email, string nome, string subject, string titulo, string message, Dictionary<string, string> dadosAdicionais, List<PerguntaRespostaMOD> listaPerguntas);
    }
}
