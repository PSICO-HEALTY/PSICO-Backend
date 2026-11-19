using PSICO.Core.Interfaces.Notificaciones;
using PSICO.Data.Strategies.Email.Enumerables;
using PSICO.Data.Strategies.Email.Interfaces;

namespace PSICO.Core.Services.Notificaciones
{
    public class SecurityNotificationService : ISecurityNotificationService
    {
        private readonly IEmailContextController _emailService;

        //private readonly ISmsService _smsService; // Por ahora no se manejan notificaciones SMS

        public SecurityNotificationService(
            IEmailContextController emailService
            )
        {
            _emailService = emailService;
        }

        public async Task NotificarAsociado(string cc, string nombre, string to)
        {
            var titulo = "Cambio de Clave";

            var mensaje = $"Usted ha realizado un cambio de contraseña de ingreso al portal.";

            var diccionario = new Dictionary<string, string>
            {
                { "NOMBREUSUARIO", $" {nombre}" },
                { "TITULO", titulo },
                { "MENSAJE", mensaje },
                { "MENSAJEAUX", "" },
                { "PASSWORDPROVISIONAL", "" },
            };

            await _emailService.Send(TemplateTypeEnum.Std, titulo, to, diccionario);
            //await _smsService.SendGeneral(cc, mensaje);
        }

        public async Task SendTokenEmail(string nombre, string to, string token)
        {
            var titulo = "Token de Seguridad";

            var diccionario = new Dictionary<string, string>
            {
                { "NOMBREUSUARIO", nombre },
                { "EMAIL", to },
                { "PASSWORDPROVISIONAL", token }
            };

            await _emailService.Send(TemplateTypeEnum.Token, titulo, to, diccionario);
        }

        public async Task<bool> SendTokenEmailSms(int option, string cc, string nombre, string email, string token)
        {
            var response = false;

            try
            {
                // Realizamos el envio por correo electronico
                if (option == 1)
                {
                    await SendTokenEmail(nombre, email, token);
                    response = true;
                }

                // Realizamos el envio por sms (futuro)
                //else if (option == 2) response = await _smsService.SendToken(cc, token);
            }

            catch (Exception ex) { }

            return response;
        }
    }
}
