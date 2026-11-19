using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PSICO.Data.DTO
{
    public enum TokenPurpose
    {
        Context,
        PasswordChange,
        PasswordReset
    }

    // Envio de token principal (Context)
    public class SendTokenRequestDto
    {
        [Range(1, 2)] // 1-Celular; 2-Email;
        public int option { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TokenPurpose purpose { get; set; } = TokenPurpose.Context;

        public string cc { get; set; }
    }

    // Envio de token para cambio de contraseña (PasswordChange) || (PasswordReset)
    public class ChangePasswordDto
    {
        public string currentPassword { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TokenPurpose purpose { get; set; }

        public string cc { get; set; }

        [Required]
        public string password { get; set; }
        [Required]
        public string ticket { get; set; }
    }

    public class PasswordChangeTicketDto
    {
        public string ticket { get; set; }
    }
}
