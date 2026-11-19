using PSICO.Data.Common;
using PSICO.Data.Strategies.Email.Enumerables;

namespace PSICO.Data.Strategies.Email.Interfaces
{
    public interface IEmailContextController
    {
        Task Send(TemplateTypeEnum template, string title, string to, Dictionary<string, string> dict,
            List<EmailAtachment> files = null, Enumerables.EmailTypeEnum emailType = Enumerables.EmailTypeEnum.Smtp);
    }
}
