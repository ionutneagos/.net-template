using Microsoft.AspNetCore.OData.Formatter;

namespace Web.Extensions
{
    internal static class RemoveFormatters
    {
        public static void RemoveODataFormatters(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                var inputFormattersToRemove = options.InputFormatters.OfType<ODataInputFormatter>().ToList();
                foreach (var formatter in inputFormattersToRemove)
                {
                    options.InputFormatters.Remove(formatter);
                }

                var outputFormattersToRemove = options.OutputFormatters.OfType<ODataOutputFormatter>().ToList();
                foreach (var formatter in outputFormattersToRemove)
                {
                    options.OutputFormatters.Remove(formatter);
                }
            });
        }
    }
}
