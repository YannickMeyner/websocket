using System.Net.WebSockets;
using System.Text;

namespace client2
{
    class Program
    {
        private static Guid clientId = Guid.Parse("c30b8358-3441-4b1f-9828-290ffc42c929"); /*Guid.NewGuid();*/
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
                        var responseBuffer = new byte[1024];
                        var offset = 0;
                        var packet = 1024;
                        var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                        WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cTs.Token);
                        var responseMsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                        Console.WriteLine(responseMsg);
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