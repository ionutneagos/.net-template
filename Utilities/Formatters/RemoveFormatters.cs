using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;

namespace Utilities.Formatters
{
    public static class RemoveFormatters
    {
        public static void RemoveODataFormatters(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                List<ODataInputFormatter> inputFormattersToRemove = options.InputFormatters.OfType<ODataInputFormatter>().ToList();
                foreach (ODataInputFormatter? formatter in inputFormattersToRemove)
                {
                    options.InputFormatters.Remove(formatter);
                }

                List<ODataOutputFormatter> outputFormattersToRemove = options.OutputFormatters.OfType<ODataOutputFormatter>().ToList();
                foreach (ODataOutputFormatter? formatter in outputFormattersToRemove)
                {
                    options.OutputFormatters.Remove(formatter);
                }
            });
        }
    }
}
