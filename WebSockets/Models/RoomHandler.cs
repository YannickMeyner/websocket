namespace WebSockets.Models
{
    public class RoomHandler
    {
        private readonly ILogger logger;
        public List<User> connectedUsers = new List<User>();

        public RoomHandler(ILogger<RoomHandler> logger)
        {
            this.logger = logger;
        }

        public void AddUser(User user)
        {
            if (!connectedUsers.Where(u => u.Id == user.Id && u.RoomId == user.RoomId).Any())
                connectedUsers.Add(user);
        }

        public void RemoveUser(User user)
        {
            connectedUsers.RemoveAll(u => u.Id == user.Id && u.RoomId == user.RoomId);
        }
    }

    public class User
    {
        public string? Id { get; set; }
        public string? RoomId { get; set; }
    }
}
