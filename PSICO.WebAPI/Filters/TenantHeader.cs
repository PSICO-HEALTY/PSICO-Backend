using Microsoft.OpenApi;
using PSICO_WebAPI.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PSICO.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TenantHeader : Attribute, ICustomAttribute, IOperationFilter
    {
        public static string HeaderName = "tenant";

        public bool IsMandatory { get; }

        public TenantHeader(bool isMandatory = false)
        {
            IsMandatory = isMandatory;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            CustomAttribute acceptedLanguageHeader = context.RequireAttribute<TenantHeader>();

            if (!acceptedLanguageHeader.ContainsAttribute)
                return;

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = HeaderName,
                In = ParameterLocation.Header,
                Required = acceptedLanguageHeader.Mandatory,
                Schema = new OpenApiSchema() { Type = JsonSchemaType.String }
            });
        }
    }
}
