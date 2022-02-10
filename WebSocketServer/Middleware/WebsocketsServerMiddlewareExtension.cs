
namespace WebSocketServer.Middleware;
public static class WebsocketServerMiddlewareExtension{

    //Extension method for exposing WebSocketServerMiddleware
    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder){
        return builder.UseMiddleware<WebsocketServerMiddleware>();
    }

    public static IServiceCollection AddWebSocketConnectionManager(this IServiceCollection service){
        return service.AddSingleton<WebSocketServerConnectionManager>();
    }
}