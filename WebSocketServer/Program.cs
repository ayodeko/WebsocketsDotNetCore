using System.Net.WebSockets;
using WebSocketServer.Middleware;;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddWebSocketConnectionManager();

var app = builder.Build();


/*Request delegates are functions used to describe our request pipelines, this is where the middleware is configured,
a request delegates reads a request and determines which logic should be performed on the request
Types are Use(), Map(), and Run().
-Use(): Used for short circuiting/ intercepting the request if it satisfies any condition and returns a response
-Map(): Used to branch out a request based on the Url format
-Run(): is often executed after all other request delegates have been executed
*/

app.UseWebSockets();
app.UseWebSocketServer();


//app.MapGet("/", () => "Hello World!");

app.Run();
