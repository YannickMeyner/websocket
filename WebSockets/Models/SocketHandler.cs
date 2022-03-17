using System.Net.WebSockets;
using System.Text;

namespace WebSockets.Models
{
    public class SocketHandler
    {
        private ILogger logger;

        public SocketHandler(ILogger<SocketHandler> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
        }

        public async Task<string?> SendToAll(HttpContext context, WebSocket webSocket, string? sender, Room room)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (response != null)
            {
                while (!response.CloseStatus.HasValue)
                {
                    var msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, response.Count));
                    logger.LogInformation($"Client:{sender} in Room sent:{room.Id}: {msg}");
                    foreach (var user in room.Users)
                    {
                        if (user.Id == sender)
                        {
                            await user.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("sender")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                        }
                        else
                        {
                            await user.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{sender} sent: {msg}")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                        }
                    }
                    response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await webSocket.CloseAsync(response.CloseStatus.Value, response.CloseStatusDescription, CancellationToken.None);
            return "closed";
        }

        public async Task<string> SendToSpecific(HttpContext context, WebSocket webSocket, string? sender, Room room, string? receiverId)
        {
            throw new NotImplementedException();
        }
    }
}
