using Microsoft.AspNetCore.Mvc;
using WebSockets.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> logger;
        private readonly RoomHandler roomHandler;

        public RoomController(ILogger<RoomController> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            roomHandler = serviceProvider.GetRequiredService<RoomHandler>();
        }

        [HttpPost("create/{roomId}")]
        public async Task<ActionResult> Create(Guid roomId, [FromBody] string userId)
        {
            try
            {
                logger.LogInformation($"trying to create room '{roomId}'...");

                var room = new Room
                {
                    Id = roomId.ToString(),
                    Creator = userId
                };
                room.AddMember(new User
                {
                    Id = userId,
                    RoomId = roomId.ToString(),
                });

                try
                {
                    roomHandler.AddRoom(room);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return BadRequest(e.Message);
                }

                logger.LogInformation($"room '{roomId}' successfully created");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured in {Request.Path.Value} with exception '{ex.Message}'");
                return BadRequest(ex);
            }
        }

        [HttpPost("join/{roomId}")]
        public async Task<ActionResult> Join(Guid roomId, [FromBody] string userId)
        {
            try
            {
                logger.LogInformation($"trying to join user '{userId}' in room '{roomId}'...");

                if (userId == null)
                    return BadRequest();

                var room = roomHandler.openRooms.Where(r => r.Id == roomId.ToString()).FirstOrDefault();

                if (room == null)
                {
                    logger.LogError($"can't join room '{roomId}' because the room doesn't exist");
                    return BadRequest($"can't join room '{roomId}' because the room doesn't exist");
                }

                try
                {
                    room.AddMember(new User
                    {
                        Id = userId,
                        RoomId = roomId.ToString(),
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return BadRequest(e.Message);
                }

                logger.LogInformation($"user '{userId}' successfully joined room '{roomId}'");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured in {Request.Path.Value} with exception '{ex.Message}'");
                return BadRequest();
            }
        }


    }
}
