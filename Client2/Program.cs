using System.Net.WebSockets;
using System.Text;

namespace client2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("press any button to continue...");
            Console.ReadLine();

            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("ws://localhost:5000/test");
                var cTs = new CancellationTokenSource();
                cTs.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await client.ConnectAsync(serviceUri, cTs.Token);
                    var n = 0;
                    while (client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("enter message to send");
                        var msg = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            var byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Client2: {msg}"));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
                            var responseBuffer = new byte[1024];
                            var offset = 0;
                            var packet = 1024;
                            while (true)
                            {
                                var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                                WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cTs.Token);
                                var responseMsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
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