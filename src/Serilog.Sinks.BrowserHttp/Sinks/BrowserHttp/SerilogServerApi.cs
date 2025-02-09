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
using Serilog.Events;

namespace Serilog.Sinks.BrowserHttp;

static class SerilogServerApi
{
    const string LevelMarker = "\"MinimumLevelAccepted\":\"";

    public const string CompactLogEventFormatMimeType = "application/vnd.serilog.clef";

    public static LogEventLevel? ReadEventInputResult(string? eventInputResult)
    {
        if (eventInputResult == null) return null;

        var startProp = eventInputResult.IndexOf(LevelMarker, StringComparison.Ordinal);
        if (startProp == -1)
            return null;

        var startValue = startProp + LevelMarker.Length;
        if (startValue >= eventInputResult.Length)
            return null;

        var endValue = eventInputResult.IndexOf('"', startValue);
        if (endValue == -1)
            return null;

        var value = eventInputResult.Substring(startValue, endValue - startValue);
        if (!Enum.TryParse(value, out LogEventLevel minimumLevel))
            return null;

        return minimumLevel;
    }
}