using System.Net.WebSockets;
using WebSockets.Models;

namespace server
{
    public class Startup
    {
        public IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddCors();

            services.AddSingleton<RoomHandler>();
            services.AddSingleton<SocketHandler>();

            services.AddMvc(option => option.EnableEndpointRouting = false);

            //Swagger
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "feedback.loop.simulator API";
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();
            var roomHandler = app.ApplicationServices.GetRequiredService<RoomHandler>();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            //Swagger
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseMvc();

            var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
            app.UseWebSockets(wsOptions);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.HasValue)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        if (context.Request.Path.Value.Contains("/room"))
                        {
                            var raw = context.Request.Path.Value.Split("room:")[1];
                            var roomId = raw.Split(';')[0];
                            var userId = raw.Split(':')[1];
                            var room = roomHandler.openRooms.Where(r => r.Id == roomId).First();
                            var roomMember = room.Users.Where(u => u.Id == userId).First();

                            //update websocket
                            roomMember.Socket = webSocket;

                            logger.LogInformation($"Client:{userId} in Room:{roomId} connected!");
                            logger.LogInformation($"Connected Clients in Room:{roomId} --> {room.Users.Where(u => u.Socket != null).Count()}");

                            var status = await room.SendMessageToAll(roomMember.Id, app.ApplicationServices);
                            //var status = await room.SendMessageToUser(userId, "88");

                            if (status != "closed")
                            {
                                await next();
                            }
                            else
                            {
                                //room.RemoveMember(new User { Id = userId });
                                //logger.LogInformation($"Client:{userId} in Room:{roomId} disconnected!");
                                //logger.LogInformation($"Connected Clients in Room:{roomId} --> {room.Users.Count}");
                            }
                        }
                    }
                }
            });
        }
    }
}
