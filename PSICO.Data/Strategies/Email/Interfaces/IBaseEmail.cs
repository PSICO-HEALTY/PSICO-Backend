using PSICO.Data.Common;
using PSICO.Data.Strategies.Email.Enumerables;

namespace PSICO.Data.Strategies.Email.Interfaces
{
    public interface IBaseEmail
    {
        Task<bool> Send(TemplateTypeEnum template, Dictionary<string, string> dict, List<EmailAtachment> files);
    }
}
