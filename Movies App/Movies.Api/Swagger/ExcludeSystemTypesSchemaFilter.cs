using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ExcludeSystemTypesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Namespace != null && (context.Type.Namespace.StartsWith("System") || context.Type.Namespace.StartsWith("Microsoft")))
        {
            schema.Properties.Clear();
        }
    }
}