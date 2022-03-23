using Microsoft.AspNetCore.Mvc;
using WebSockets.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly RoomHandler roomHandler;

        public RoomController(ILogger<RoomController> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            roomHandler = serviceProvider.GetRequiredService<RoomHandler>();
        }

        [HttpPost("create/{roomId}")]
        public async Task<ActionResult> Create(Guid roomId, [FromBody] string userId)
        {
            try
            {
                roomHandler.AddRoom(new Room(serviceProvider)
                {
                    Id = roomId.ToString(),
                    Creator = userId
                });

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
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occured in {Request.Path.Value} with exception '{ex.Message}'");
                return BadRequest(ex);
            }
        }
    }
}
