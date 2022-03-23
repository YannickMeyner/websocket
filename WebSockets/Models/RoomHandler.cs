using System.Net.WebSockets;

namespace WebSockets.Models
{
    public class RoomHandler
    {
        private readonly ILogger logger;
        public List<Room> openRooms = new List<Room>();

        public RoomHandler(ILogger<RoomHandler> logger)
        {
            this.logger = logger;
        }

        public void AddRoom(Room room)
        {
            openRooms.Add(room);
        }
    }

    public class Room
    {
        private readonly IServiceProvider serviceProvider;

        public string? Id { get; set; }
        public string? Creator { get; set; }
        public List<User> Users = new List<User>();

        public Room(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void AddMember(User user)
        {
            Users.Add(user);
        }

        public void RemoveMember(User user)
        {
            Users.RemoveAll(u => u.Id == user.Id);
        }

        public async Task<string?> SendMessageToAll(string? sender)
        {
            return await serviceProvider.GetRequiredService<SocketHandler>().SendToAll(sender, this);
        }

        public async Task<string?> SendMessageToUser(string? sender, string? receiver)
        {
            try
            {
                return await serviceProvider.GetRequiredService<SocketHandler>().SendToSpecific(sender, this, receiver);
            }
            catch (Exception ex) { return null; }
        }

        public void Receive()
        {

        }
    }

    public class User
    {
        public string? Id { get; set; }
        public string? RoomId { get; set; }
        public WebSocket? Socket { get; set; }
    }
}
