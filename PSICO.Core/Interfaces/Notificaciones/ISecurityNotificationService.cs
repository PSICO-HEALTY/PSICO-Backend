using System;
using System.Collections.Generic;
using System.Text;

namespace PSICO.Core.Interfaces.Notificaciones
{
    public interface ISecurityNotificationService
    {
        Task<bool> SendTokenEmailSms(int option, string cc, string nombre, string email, string token);
    }
}
