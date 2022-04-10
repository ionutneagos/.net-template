namespace Web.Filter
{
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Linq;
    using System.Reflection;
    using Web.Attribute;
    using static System.Char;

    public class SwaggerExcludeFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                System.Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(n => schema.Enum.Add(new OpenApiString(n)));
            }

            if (schema?.Properties == null)
                return;

            var excludedProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                string propertyName = $"{ToLowerInvariant(excludedProperty.Name[0])}{excludedProperty.Name.Substring(1)}";
                if (schema.Properties.ContainsKey(propertyName))
                    schema.Properties.Remove(propertyName);
            }
        }
    }
}
