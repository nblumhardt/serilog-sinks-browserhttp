using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.AspNetCore.Ingestion
{
    class SerilogIngestionMiddleware
    {
        readonly string _originPropertyName;
        readonly string _endpointPath;
        readonly ILogger _log;
        readonly long? _eventBodyLimitBytes;
        readonly LoggingLevelSwitch _clientLevelSwitch;

        public SerilogIngestionMiddleware(SerilogIngestionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _endpointPath = options.EndpointPath ?? throw new ArgumentException("An ingestion endpoint must be specified.");

            _log = options.Logger ?? Log.Logger;
            if (options.OriginPropertyName != null)
            {
                _originPropertyName = options.OriginPropertyName;
                _log = _log.ForContext(_originPropertyName, "Client");
            }

            _eventBodyLimitBytes = options.EventBodyLimitBytes;
            _clientLevelSwitch = options.ClientLevelSwitch;
        }
        
        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path != _endpointPath)
            {
                await next();
                return;
            }

            var reader = new StreamReader(context.Request.Body);

            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (line.Length > _eventBodyLimitBytes)
                    {
                        var startToLog = (int)Math.Min(_eventBodyLimitBytes ?? 1024, 1024);
                        var prefix = line.Substring(0, startToLog);
                        SelfLog.WriteLine("Dropping oversize event from {0} of {1} chars: {2}", context.Connection.RemoteIpAddress, line.Length, prefix);
                        line = reader.ReadLine();
                        continue;
                    }

                    try
                    {
                        var jObject = JsonConvert.DeserializeObject<JObject>(line);
                        var evt = LogEventReader.ReadFromJObject(jObject);
                        if (_clientLevelSwitch == null || evt.Level >= _clientLevelSwitch.MinimumLevel)
                        {
                            if (_originPropertyName != null)
                                evt.RemovePropertyIfPresent(_originPropertyName); // Ensure the client can't override this
    
                            _log.Write(evt);
                        }
                    }
                    catch (Exception ex)
                    {
                        SelfLog.WriteLine("Failed to deserialize event from {0}: {1}", context.Connection.RemoteIpAddress, ex);
                    }
                }

                line = await reader.ReadLineAsync();
            }

            context.Response.StatusCode = 201;
            if (_clientLevelSwitch != null)
            {
                await context.Response.WriteAsync("{\"MinimumLevelAccepted\":\"" + _clientLevelSwitch.MinimumLevel + "\"}");
            }
        }
    }
}
