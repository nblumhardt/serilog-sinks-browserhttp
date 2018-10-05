// Copyright 2016-2018 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net.Http;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace SerilogBlazorDemo.Client.Diagnostics
{
    /// <summary>
    /// Extends Serilog configuration to write events via HTTP.
    /// </summary>
    public static class LoggerConfigurationBrowserHttpExtensions
    {
        const string DefaultOriginEndpointUrl = "serilog";

        /// <summary>
        /// Adds a sink that writes log events to an HTTP server.
        /// </summary>
        /// <param name="loggerSinkConfiguration">The logger configuration.</param>
        /// <param name="endpointUrl">The URL of the server logging endpoint.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required 
        /// in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="bufferBaseFilename">Path for a set of files that will be used to buffer events until they
        /// can be successfully transmitted across the network. Individual files will be created using the
        /// pattern <paramref name="bufferBaseFilename"/>*.json, which should not clash with any other filenames
        /// in the same directory.</param>
        /// <param name="bufferSizeLimitBytes">The maximum amount of data, in bytes, to which the buffer
        /// log file for a specific date will be allowed to grow. By default no limit will be applied.</param>
        /// <param name="eventBodyLimitBytes">The maximum size, in bytes, that the JSON representation of
        /// an event may take before it is dropped rather than being sent to the Seq server. Specify null for no limit.
        /// The default is 265 KB.</param>
        /// <param name="controlLevelSwitch">If provided, the switch will be updated based on the Seq server's level setting
        /// for the corresponding API key. Passing the same key to MinimumLevel.ControlledBy() will make the whole pipeline
        /// dynamically controlled. Do not specify <paramref name="restrictedToMinimumLevel"/> with this setting.</param>
        /// <param name="messageHandler">Used to construct the HttpClient that will send the log messages to Seq.</param>
        /// <param name="queueSizeLimit">The maximum number of events that will be held in-memory while waiting to ship them to
        /// Seq. Beyond this limit, events will be dropped. The default is 100,000. Has no effect on
        /// durable log shipping.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration BrowserHttp(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string endpointUrl = DefaultOriginEndpointUrl,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = BrowserHttpSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            string bufferBaseFilename = null,
            long? bufferSizeLimitBytes = null,
            long? eventBodyLimitBytes = 256 * 1024,
            LoggingLevelSwitch controlLevelSwitch = null,
            HttpMessageHandler messageHandler = null,
            int queueSizeLimit = BrowserHttpSink.DefaultQueueSizeLimit)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            if (endpointUrl == null) throw new ArgumentNullException(nameof(endpointUrl));
            if (bufferSizeLimitBytes.HasValue && bufferSizeLimitBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSizeLimitBytes), "Negative value provided; buffer size limit must be non-negative.");
            if (queueSizeLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(queueSizeLimit), "Queue size limit must be non-zero.");

            var defaultedPeriod = period ?? BrowserHttpSink.DefaultPeriod;

            var sink = new BrowserHttpSink(
                endpointUrl,
                batchPostingLimit,
                defaultedPeriod,
                eventBodyLimitBytes,
                controlLevelSwitch,
                queueSizeLimit);

            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
