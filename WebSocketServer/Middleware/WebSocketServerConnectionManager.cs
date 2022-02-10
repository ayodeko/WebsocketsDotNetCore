using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocketServer.Middleware;
public class WebSocketServerConnectionManager{
    ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket> ();

    public ConcurrentDictionary<string, WebSocket> GetAllSockets(){
        return _sockets;
    }

    public string AdSocket(WebSocket socket){
        var connID = Guid.NewGuid().ToString();
        if(_sockets.TryAdd(connID, socket)){
            Console.WriteLine("Added new socket to connection, ID: " + connID);
            return connID;
        }
        Console.WriteLine("Failed to add new socket");
        return "Error adding new socket";
    }

    public WebSocket GetSocketById(string connId, out bool status){
        status = _sockets.TryGetValue(connId, out var socket);
        if(socket == null){
            Console.WriteLine("Socket retrieved is null");
            return socket;
        }
        return socket;
    }

    
    public string GetIdBySocket(WebSocket socket, out bool status){
        var id = _sockets.FirstOrDefault(c => c.Value == socket).Key;
        if(string.IsNullOrEmpty(id)){
            status = false;
            Console.WriteLine("Socket retrieved is null");
            return id;
        }
        status = true;
        return id;
    }

}
