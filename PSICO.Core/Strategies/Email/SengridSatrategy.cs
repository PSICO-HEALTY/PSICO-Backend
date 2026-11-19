using PSICO.Data.Common;
using PSICO.Data.Strategies.Email;
using PSICO.Data.Strategies.Email.Enumerables;
using PSICO.Data.Strategies.Email.Interfaces;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace PSICO.Core.Strategies.Email
{
    public class SengridSatrategy : BaseEmail, IBaseEmail
    {
        private readonly string _sengridApiKey;
        private readonly string _sengridEmailFrom;
        private readonly string _to;
        private readonly string _title;

        public SengridSatrategy(
            string sengridApiKey,
            string sengridEmailFrom,
            string to,
            string title)
        {
            _sengridApiKey = sengridApiKey;
            _sengridEmailFrom = sengridEmailFrom;
            _to = to;
            _title = title;
        }

        public async Task<bool> Send(TemplateTypeEnum template, Dictionary<string, string> dict, List<EmailAtachment> files)
        {
            var result = false;

            try
            {
                var client = new SendGridClient(_sengridApiKey);
                var from = new EmailAddress(_sengridEmailFrom);
                var to = new EmailAddress(_to);

                var msg = new SendGridMessage
                {
                    From = from,
                    Subject = _title,
                    HtmlContent = GetPlantillaHtml(dict, template)
                };
                msg.AddTo(to);

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        msg.AddAttachment(new Attachment()
                        {
                            Filename = file.Name,
                            Content = file.Base64Content
                        });
                    }
                }
                await client.SendEmailAsync(msg);

                result = true;
            }
            catch(Exception ex) { }

            return result;
        }
    }
}
