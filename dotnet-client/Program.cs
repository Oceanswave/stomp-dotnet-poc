namespace dotnet_core_client
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using dotnet_core_client.StompClient;

    class Program
    {
        //TODO: Pull these from environment variables
        private const string NextJSTopic = "hello-from-next-js";
        private const string DotNetTopic = "hello-from-dot-net";

        private static readonly Uri BrokerUri = new Uri("ws://rabbitmq:15674/ws");

        private const string User = "guest";
        private const string Passcode = "guest";

        static void Main(string[] args)
        {
            using (IStompClient client = new StompWebsocketClient(BrokerUri))
            {
                Console.WriteLine("Connecting...");
                client.Connect(new Dictionary<string, string>()
                {
                    { "User", User },
                    { "Passcode", Passcode },
                });
                Console.WriteLine("Connected.");

                client.Subscribe<string>($"/topic/{NextJSTopic}", new Dictionary<string, string>(), SayHello);

                Console.WriteLine("Press escape to stop.");
                do
                {
                    var input = Console.In.Peek();
                    while (input == -1)
                    {
                        Console.WriteLine("Saying Hello...");
                        client.Send("Hello from dotnet core!!", $"/topic/{DotNetTopic}", new Dictionary<string, string>());
                        Thread.Sleep(2000);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
        }

        private static void SayHello(object sender, string content)
        {
            Console.WriteLine($"NextJS says: {content}");
        }
    }
}
