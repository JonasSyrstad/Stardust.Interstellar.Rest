using System;
using System.Text;
using System.Threading.Tasks;
using Swashbuckle.Application;

namespace Swashbuckle.Stardust.Interstellar
{
    public static class SwaggerExtensions
    {
        public static SwaggerDocsConfig ConfigureStardust(this SwaggerDocsConfig swaggerConfig, Action<SwaggerDocsConfig> additionalConfigurations)
        {
            swaggerConfig.OperationFilter(() => new StardustOperationDescriptor());
            return swaggerConfig;
        }

        public static SwaggerDocsConfig ConfigureStardust(this SwaggerDocsConfig swaggerConfig)
        {
            swaggerConfig.OperationFilter(() => new StardustOperationDescriptor());
            return swaggerConfig;
        }
    }
}
