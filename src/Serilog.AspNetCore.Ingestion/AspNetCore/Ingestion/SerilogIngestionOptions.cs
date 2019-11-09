using Serilog.Core;

namespace Serilog.AspNetCore.Ingestion
{
    public class SerilogIngestionOptions
    {
        public string EndpointPath { get; set; } = "/ingest";
        public string OriginPropertyName { get; set; } = "Origin";
        public ILogger Logger { get; set; }
        public long? EventBodyLimitBytes { get; set; }
        public LoggingLevelSwitch ClientLevelSwitch { get; set; }
    }
}
