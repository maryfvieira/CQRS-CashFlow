using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CashFlow.ConsolidationApi.Filters;

public class BrazilianDateSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(DateTime)) return;
        
        schema.Format = "dd/MM/yyyy";
        schema.Example = new OpenApiString(DateTime.Now.Date.ToString(schema.Format));
    }
}
