using System;
using Microsoft.AspNetCore.Builder;
using Serilog.AspNetCore.Ingestion;

namespace Serilog
{
    public static class ApplicationBuilderSerilogClientExtensions
    {
        public static IApplicationBuilder UseSerilogIngestion(
            this IApplicationBuilder app,
            Action<SerilogIngestionOptions> configureOptions = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var options = new SerilogIngestionOptions();
            configureOptions?.Invoke(options);
            
            var sm = new SerilogIngestionMiddleware(options);
            return app.Use(sm.Invoke);
        }
    }
}
