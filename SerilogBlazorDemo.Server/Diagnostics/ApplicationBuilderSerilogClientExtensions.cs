using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Core;

namespace SerilogBlazorDemo.Server.Diagnostics
{
    public static class ApplicationBuilderSerilogClientExtensions
    {
        public static IApplicationBuilder UseSerilogClient(
            this IApplicationBuilder app,
            string endpointPath = "/serilog",
            ILogger log = null,
            long? eventBodyLimitBytes = null,
            LoggingLevelSwitch clientLevelSwitch = null)
        {
            var sm = new SerilogClientLoggingMiddleware(endpointPath, log, eventBodyLimitBytes, clientLevelSwitch);
            return app.Use(sm.Invoke);
        }
    }
}
