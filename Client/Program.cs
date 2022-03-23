﻿using System.Net.WebSockets;
using System.Text;

namespace client
{
    class Program
    {
        private static Guid clientId = Guid.Parse("6458c64f-cdd4-4d23-ae12-feaaaac4ba01"); /*Guid.NewGuid();*/
        private static Guid roomId = Guid.Parse("35fb1aa2-2849-4f1f-870c-203e38f1cc0d"); /*1234;*/

        static async Task Main(string[] args)
        {
            Uri serviceUri = new Uri($"ws://localhost:5000/room:{roomId};user:{clientId}");

            Console.WriteLine($"press any button to connect to socket-address {serviceUri.AbsolutePath}...");
            Console.ReadLine();

            using (ClientWebSocket client = new ClientWebSocket())
            {
                var cTs = new CancellationTokenSource();
                cTs.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await client.ConnectAsync(serviceUri, cTs.Token);
                    while (client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("enter message to send");
                        var msg = Console.ReadLine();

                        if (msg == "close")
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "client disconnected", CancellationToken.None);
                            Environment.Exit(1);
                        }

                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            var byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{msg}"));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
                            var responseBuffer = new byte[1024];
                            var offset = 0;
                            var packet = 1024;
                            while (true)
                            {
                                var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                                WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cTs.Token);
                                var responseMsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                                if (responseMsg != "sender")
                                    Console.WriteLine(responseMsg);
                                if (response.EndOfMessage)
                                    break;
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.ReadLine();
        }
    }
}