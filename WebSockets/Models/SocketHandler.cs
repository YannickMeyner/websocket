using System.Net.WebSockets;
using System.Text;

namespace WebSockets.Models
{
    public class SocketHandler
    {
        private ILogger logger;
        private readonly RoomHandler roomHandler;

        public SocketHandler(ILogger<SocketHandler> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            roomHandler = serviceProvider.GetRequiredService<RoomHandler>();
        }

        public async Task<string?> RoomService(HttpContext context, WebSocket webSocket, string? userId, string? roomId)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (response != null)
            {
                while (!response.CloseStatus.HasValue)
                {
                    var msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, response.Count));
                    logger.LogInformation($"Client:{userId} in Room sent:{roomId}: {msg}");
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"success!")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                    response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await webSocket.CloseAsync(response.CloseStatus.Value, response.CloseStatusDescription, CancellationToken.None);
            return "closed";
        }
    }
}
