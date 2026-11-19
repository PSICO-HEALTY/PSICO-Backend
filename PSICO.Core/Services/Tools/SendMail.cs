using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using PSICO.Core.Services.Tools.Interface;
using PSICO.Data.Common;

namespace PSICO.Core.Services.Tools
{
    public class SendMail : ISendMail
    {
        public bool validar_Mail(string sMail)
        {
            return Regex.IsMatch(sMail.Trim(),
                @"^([\w-]+\.)*?[\w-]+@[\w-]+\.([\w-]+\.)*?[\w]+$");
        }

        public bool validaEmails(string email)
        {
            string[] destinatarios = email.Split(';');
            bool validar = false;

            foreach (string destinatario in destinatarios)
            {
                validar = validar_Mail(destinatario);
            }

            return validar;
        }

        public bool EnviarCorreoSolido(string ServerSmtp, string PasswordEnvio, string CorreoEnvio,
            string Destinatario, string Asunto, string Mensaje, List<EmailAtachment>? ArchivosAdjuntos = null,
            int puerto = 587, bool SSL = true, string msg = "")
        {
            string[] destinatarios = Destinatario.Split(';');
            using var mail = new MailMessage
            {
                From = new MailAddress(CorreoEnvio),
                Subject = Asunto,
                Body = Mensaje,
                IsBodyHtml = true,
                Priority = MailPriority.Normal,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };

            mail.To.Add(destinatarios[0]);
            for (int i = 1; i < destinatarios.Length; i++)
            {
                mail.CC.Add(destinatarios[i]);
            }

            if (ArchivosAdjuntos != null)
            {
                foreach (EmailAtachment archivo in ArchivosAdjuntos)
                {
                    var stream = new MemoryStream(Convert.FromBase64String(archivo.Base64Content));
                    mail.Attachments.Add(new Attachment(stream, archivo.Name));
                }
            }

            using var smtp = new SmtpClient(ServerSmtp)
            {
                UseDefaultCredentials = false,
                Port = puerto,
                Credentials = new NetworkCredential(CorreoEnvio, PasswordEnvio),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = SSL
            };
            // Pendiente: registrar los errores cuando se implemente el logger.
            smtp.Send(mail);
            return true;
        }
    }
}
