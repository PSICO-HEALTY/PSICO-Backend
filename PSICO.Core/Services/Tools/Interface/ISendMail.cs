using PSICO.Data.Common; 

namespace PSICO.Core.Services.Tools.Interface
{
    public interface ISendMail
    {
        bool validaEmails(string email);

        bool EnviarCorreoSolido(string ServerSmtp, string PasswordEnvio, string CorreoEnvio,
            string Destinatario, string Asunto, string Mensaje, List<EmailAtachment>? ArchivosAdjuntos = null,
            int puerto = 587, bool SSL = true, string msg = "");
    }
}
