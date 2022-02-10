using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace WebSocketServer.Middleware;

//Middleware class for the WebsocketServerMiddlewareExtension
public class WebsocketServerMiddleware{
    private readonly RequestDelegate _next;
    private readonly WebSocketServerConnectionManager _socketManager;

    //In creating an extension, the middleware class must have a constructor that takes in a RequestDelegate
    public WebsocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager socketManager)
    {
        _next = next;
        _socketManager = socketManager;
    }

// When the WebSocketExtension is called in program.cs, it looks in the middleware class for any method with the name
//Invoke or InvokeAsync and takes in HttpContext. This method is the entry point for the extension in program.cs
    public async Task InvokeAsync(HttpContext context){
        try{
		ViewRequestHeaders(context);
	if (context.WebSockets.IsWebSocketRequest)
	{
		var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connId = _socketManager.AdSocket(webSocket);
		await SendConnIDAsync(webSocket, connId);
		await ReceiveMessageAsync(webSocket, async (result, buffer) => {
			if(result.MessageType == WebSocketMessageType.Text){
				Console.WriteLine("Text received");
				var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Text Message: {receivedText}");

				if(receivedText.Contains(":")){
					await RouteMessageAsync(webSocket, receivedText);
					Console.WriteLine("Agree to specified condition");
				}
				else{
					Console.WriteLine("Does not agree to specified condition");
				}
			}
			else if(result.MessageType == WebSocketMessageType.Close){
				Console.WriteLine("Command received to close webSocket ");
				var socketId = _socketManager.GetIdBySocket(webSocket, out var status);
				await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
				_socketManager.GetAllSockets().Remove(socketId, out var val);

		}
		});
	}
	else
	{
		await _next(context);
	}
	}
	catch(Exception ex){
		Console.WriteLine(Convert.ToString(ex));
	}
    }

    void ViewRequestHeaders(HttpContext context){

	Console.WriteLine($"Request Method: {context.Request.Method}");
	Console.WriteLine($"Request Protocol: {context.Request.Protocol}");
	foreach(var header in context.Request.Headers){
		Console.WriteLine($"HeaderKey: {header.Key}, HeaderValue: {header.Value}");
	}
}

async Task RouteMessageAsync(WebSocket socket, string jsonMessage){
	var objectFormat = new {
		To = "",
		From = "",
		Message = ""
	};
	var message = JsonConvert.DeserializeAnonymousType(jsonMessage, objectFormat);
	await SendMessageAsync(message.To, message.Message);
}

async Task SendMessageAsync(string connId, string message){
	var socket = _socketManager.GetSocketById(connId, out var status);
	if(status == false){ Console.WriteLine($"Socket not available for connId {connId}"); return;}
	Console.WriteLine($"Sending message {message} to {connId}");
	var buffer = Encoding.UTF8.GetBytes(message);
	await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
}

async Task SendConnIDAsync(WebSocket socket, string connID){
	var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
	await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
}

async Task ReceiveMessageAsync(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage){
	var buffer = new byte[1024 * 4];
	while(socket.State == WebSocketState.Open){
	var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
	handleMessage(result, buffer);
	}

}
}