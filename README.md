# serilog-blazor

Experimental/proof-of-concept Serilog extensions for Blazor.

Run the `SerilogBlazorDemo.Server` project as a console app (the _SerilogBlazorDemo.Server_ configuration, not IIS):

 * Events are written to the browser console on the client, and,
 * To the console on the server, tagged with `Origin: Client`.

Clicking the counter button in the browser will trigger some more logging.

