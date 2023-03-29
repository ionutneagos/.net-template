using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using static System.Char;

namespace Utilities.Swagger
{
    public class SwaggerExcludeFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(n => schema.Enum.Add(new OpenApiString(n)));
            }

            if (schema?.Properties == null)
                return;

            IEnumerable<PropertyInfo> excludedProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

            foreach (PropertyInfo excludedProperty in excludedProperties)
            {
                string propertyName = $"{ToLowerInvariant(excludedProperty.Name[0])}{excludedProperty.Name.Substring(1)}";
                if (schema.Properties.ContainsKey(propertyName))
                    schema.Properties.Remove(propertyName);
            }
        }
    }
}
