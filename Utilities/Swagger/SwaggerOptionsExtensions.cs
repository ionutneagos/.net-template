using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Utilities.Swagger
{
    public static class SwaggerOptionsExtensions
    {
        public static void LoadXmlComments(this SwaggerGenOptions swaggerGenOptions, string? applicationRootPath = null, string? searchPattern = null)
        {
            applicationRootPath ??= AppContext.BaseDirectory;
            searchPattern ??= "*.xml";
            foreach (string filePath in Directory.GetFiles(applicationRootPath, searchPattern))
            {
                try
                {
                    swaggerGenOptions.IncludeXmlComments(filePath, true);
                }
                catch
                {
                    //Intentionaly left empty
                }
            }
        }
    }
}
