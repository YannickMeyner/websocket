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
            if (room == null)
                throw new Exception("room object cannot be null");

            var alreadyExisting = openRooms.Where(r => r.Id == room.Id).FirstOrDefault();
            if (alreadyExisting != null)
                throw new Exception($"cannot create room because room '{room.Id}' already exists");

            openRooms.Add(room);
        }
    }

    public class Room
    {
        public string? Id { get; set; }
        public string? Creator { get; set; }
        public List<User> Users = new List<User>();

        public Room() { }

        public void AddMember(User user)
        {
            if (user == null)
                throw new Exception("user object cannot be null");

            var userAlreadyExsting = Users.Where(u => u.Id == user.Id).FirstOrDefault();
            if (userAlreadyExsting != null)
                throw new Exception($"cannot add user because user '{user.Id}' is already a member of this room");

            Users.Add(user);
        }

        public void RemoveMember(User user)
        {
            Users.RemoveAll(u => u.Id == user.Id);
        }

        public async Task<string?> SendMessageToAll(string? sender, IServiceProvider serviceProvider)
        {
            return await serviceProvider.GetRequiredService<SocketHandler>().SendToAll(sender, this);
        }

        public async Task<string?> SendMessageToUser(string? sender, string? receiver, IServiceProvider serviceProvider)
        {
            try
            {
                return await serviceProvider.GetRequiredService<SocketHandler>().SendToSpecific(sender, this, receiver);
            }
            catch (Exception ex) { return null; }
        }
    }

    public class User
    {
        public string? Id { get; set; }
        public string? RoomId { get; set; }
        public WebSocket? Socket { get; set; }
    }
}
