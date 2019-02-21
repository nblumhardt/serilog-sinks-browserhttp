using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Formatting.Compact.Reader;

namespace SerilogBlazorDemo.Server.Diagnostics
{
    class SerilogClientLoggingMiddleware
    {
        const string OriginPropertyName = "Origin";

        readonly string _endpointPath;
        readonly ILogger _log;
        readonly long? _eventBodyLimitBytes;
        readonly LoggingLevelSwitch _clientLevelSwitch;

        public SerilogClientLoggingMiddleware(
            string endpointPath,
            ILogger log,
            long? eventBodyLimitBytes,
            LoggingLevelSwitch clientLevelSwitch)
        {
            _endpointPath = endpointPath;
            _log = (log ?? Log.Logger).ForContext(OriginPropertyName, "Client");
            _eventBodyLimitBytes = eventBodyLimitBytes;
            _clientLevelSwitch = clientLevelSwitch;
        }
        
        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path != _endpointPath)
            {
                await next();
                return;
            }

            var reader = new StreamReader(context.Request.Body);

            var line = reader.ReadLine();
            while (line != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (line.Length > _eventBodyLimitBytes)
                    {
                        var startToLog = (int)Math.Min(_eventBodyLimitBytes ?? 1024, 1024);
                        var prefix = line.Substring(0, startToLog);
                        SelfLog.WriteLine("Dropping oversized event from {0} of {1} chars: {2}", context.Connection.RemoteIpAddress, line.Length, prefix);
                        line = reader.ReadLine();
                        continue;
                    }

                    try
                    {
                        var jobj = JsonConvert.DeserializeObject<JObject>(line);
                        var evt = LogEventReader.ReadFromJObject(jobj);
                        if (_clientLevelSwitch == null || evt.Level >= _clientLevelSwitch.MinimumLevel)
                        {
                            evt.RemovePropertyIfPresent(OriginPropertyName); // Ensure the client can't override this
                            _log.Write(evt);
                        }
                    }
                    catch (Exception ex)
                    {
                        SelfLog.WriteLine("Failed to deserialize event from {0}: {1}", context.Connection.RemoteIpAddress, ex);
                    }
                }

                line = reader.ReadLine();
            }

            context.Response.StatusCode = 201;
            if (_clientLevelSwitch != null)
            {
                await context.Response.WriteAsync("{\"MinimumLevelAccepted\":\"" + _clientLevelSwitch.MinimumLevel + "\"}");
            }
        }
    }
}
