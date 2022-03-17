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

        public async Task<string?> SendToAll(string? senderId, Room room)
        {
            var sender = room.Users.Where(u => u.Id == senderId).First();

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult response = await sender.Socket!.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (response != null)
            {
                while (!response.CloseStatus.HasValue)
                {
                    var msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, response.Count));
                    logger.LogInformation($"Client:{sender.Id} in Room sent:{room.Id}: {msg}");
                    foreach (var user in room.Users)
                    {
                        if (user.Id == sender.Id)
                        {
                            await user.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("sender")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                        }
                        else
                        {
                            await user.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{sender.Id} sent: {msg}")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                        }
                    }
                    response = await sender.Socket!.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await sender.Socket!.CloseAsync(response.CloseStatus.Value, response.CloseStatusDescription, CancellationToken.None);
            return "closed";
        }

        public async Task<string?> SendToSpecific(string? senderId, Room room, string? receiverId)
        {
            var sender = room.Users.Where(u => u.Id == senderId).First();

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult response = await sender.Socket!.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (response != null)
            {
                while (!response.CloseStatus.HasValue)
                {
                    var receiver = room.Users.Where(u => u.Id == receiverId).FirstOrDefault();
                    var msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, response.Count));

                    if (receiver != null)
                    {
                        logger.LogInformation($"Client:{sender} in Room sent:{room.Id}: {msg}");
                        await receiver.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{sender.Id} sent: {msg}")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                    }
                    else
                    {
                        logger.LogInformation($"Message could not be sent because 'receiver {receiverId}' doesn't exist in room '{room.Id}'");
                    }

                    await sender.Socket!.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("sender")), response.MessageType, response.EndOfMessage, CancellationToken.None);
                    response = await sender.Socket!.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            await sender.Socket!.CloseAsync(response.CloseStatus.Value, response.CloseStatusDescription, CancellationToken.None);
            return "closed";
        }
    }
}
