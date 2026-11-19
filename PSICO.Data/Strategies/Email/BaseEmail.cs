using PSICO.Data.Strategies.Email.Enumerables;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PSICO.Data.Strategies.Email
{
    public abstract class BaseEmail
    {
        private string SetCamposHtml(Dictionary<string, string> diccionario, string textoEmail)
        {
            foreach (var q in diccionario)
                textoEmail = textoEmail.Replace("{" + q.Key + "}", q.Value);

            return textoEmail;
        }

        protected string GetPlantillaHtml(Dictionary<string, string> diccionario, TemplateTypeEnum template)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase!.ToString())!.Remove(0, 6);

            string path = template switch
            {
                TemplateTypeEnum.Token => basePath + "\\EmailTemplates\\TokenDigital.html",
                TemplateTypeEnum.Std => basePath + "\\EmailTemplates\\Estandar.html",
                _ => basePath + "\\EmailTemplates\\Estandar.html"
            };

            var textoEmail = File.ReadAllText(path);
            return SetCamposHtml(diccionario, textoEmail);
        }
    }
}
