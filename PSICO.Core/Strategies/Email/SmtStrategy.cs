using PSICO.Core.Services.Tools;
using PSICO.Core.Services.Tools.Interface;
using PSICO.Data.Common;
using PSICO.Data.Strategies.Email;
using PSICO.Data.Strategies.Email.Enumerables;
using PSICO.Data.Strategies.Email.Interfaces;

namespace PSICO.Core.Strategies.Email
{
    public class SmtStrategy :  BaseEmail, IBaseEmail
    {
        private readonly ISendMail _sendSmtp;
        private readonly string _serverSMTP;
        private readonly string _pass;
        private readonly string _from;
        private readonly int _port;
        private readonly bool _ssl;
        private readonly string _to;
        private readonly string _title;

        public SmtStrategy(
            string serverSMTP,
            string pass,
            string from,
            int port,
            bool ssl,
            string to,
            string title)
        {
            _sendSmtp = new SendMail();
            _serverSMTP = serverSMTP;
            _pass = pass;
            _from = from;
            _to = to;
            _title = title;
            _port = port;
            _ssl = ssl;
        }

        public async Task<bool> Send(TemplateTypeEnum template, Dictionary<string, string> dict, List<EmailAtachment> files)
        {
            var result = false;

            if (_sendSmtp.validaEmails(_to))

                // TODO: Files as Attachments

                result = await Task.Run(() =>
                    _sendSmtp.EnviarCorreoSolido(_serverSMTP, _pass, _from, _to, _title,
                    GetPlantillaHtml(dict, template), files, _port, _ssl)
                );

            return result;
        }
    }
}
