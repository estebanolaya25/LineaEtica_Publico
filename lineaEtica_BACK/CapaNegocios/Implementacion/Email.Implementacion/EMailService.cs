using CapaDto.Common;
using CapaNegocios.Interfaz.Email.Interfaz;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace CapaNegocios.Implementacion.Email.Implementacion
{
    public class EMailService : IEMailService
    {
        private readonly string host, userName, password, fromAddress, emailName;
        private readonly int port;
        private readonly bool enableSsl;

        public EMailService(IConfiguration configuration)
        {
            host = configuration["EmailConfig:Host"] ?? "smtp.gmail.com";
            port = Convert.ToInt32(configuration["EmailConfig:Port"] ?? "587");
            userName = configuration["EmailConfig:UserName"] ?? "";
            password = configuration["EmailConfig:Password"] ?? "";
            fromAddress = configuration["EmailConfig:MailAdress"] ?? userName;
            emailName = configuration["EmailConfig:EmailName"] ?? "Sistema";
            enableSsl = bool.TryParse(configuration["EmailConfig:EnableSSL"], out bool ssl) && ssl;
        }

        public async Task<string> SendMail(EmailMessageRequestDto emailMessage)
        {
            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSsl
                };

                var from = new MailAddress(fromAddress, emailName);
                var mail = new MailMessage { From = from, Subject = emailMessage.Subject ?? "", Body = emailMessage.Body ?? "", IsBodyHtml = true };

                foreach (var to in emailMessage.To ?? [])
                    mail.To.Add(to);

                foreach (var cc in emailMessage.ToCC ?? [])
                    mail.CC.Add(cc);

                if (emailMessage.AttachmentsRoutes != null)
                    foreach (var path in emailMessage.AttachmentsRoutes.Where(File.Exists))
                        mail.Attachments.Add(new Attachment(path));

                await client.SendMailAsync(mail);
                return "OK";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
