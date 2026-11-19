using Microsoft.Extensions.Options;
using PSICO.Data.Common;
using PSICO.Data.Strategies.Email.Enumerables;
using PSICO.Data.Strategies.Email.Interfaces;

namespace PSICO.Core.Strategies.Email
{

    public class EmailContextController : IEmailContextController
    {
        private readonly GlobalParams _globalParams;
        private readonly AppSettings _dynamicParams;

        public EmailContextController(
            IOptions<GlobalParams> globalParams,
            AppSettings dynamicParams)
        {
            _globalParams = globalParams.Value;
            _dynamicParams = dynamicParams;
        }

        public async Task Send(
            TemplateTypeEnum template,
            string title,
            string to,
            Dictionary<string, string> dict,
            List<EmailAtachment> files = null,
            EmailTypeEnum emailType = EmailTypeEnum.Smtp)
        {
            IBaseEmail emailStrategy = emailType switch
            {
                EmailTypeEnum.Smtp => new SmtStrategy(
                                        _dynamicParams.ServerSmtp, _dynamicParams.PasswordEnvio, _dynamicParams.CorreoEnvio,
                                        _dynamicParams.puertoSMTP, _dynamicParams.EnabledSSL, to, title),

                EmailTypeEnum.Sendgrid => new SengridSatrategy(_globalParams.SengridApiKey, _globalParams.SengridEmailFrom, to, title),

                _ => new SengridSatrategy(_globalParams.SengridApiKey, _globalParams.SengridEmailFrom, to, title),
            };

            dict.Add("NOMBREEMPRESA", _globalParams.AppName);
            dict.Add("YEAR", DateTime.Now.Year.ToString());

            // Send email using strategy selected
            var defaultTry = await emailStrategy.Send(template, dict, files);

            // Case when default try failed and strategy selected was not sendgrid
            if (!defaultTry && emailType != EmailTypeEnum.Sendgrid)
            {
                emailStrategy = new SengridSatrategy(_globalParams.SengridApiKey, _globalParams.SengridEmailFrom, to, title);
                await emailStrategy.Send(template, dict, files);
            }
        }
    }
}
