namespace dotnet_core_client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using dotnet_core_client.StompClient;

    class Program
    {
        //TODO: Pull these from environment variables
        private const string NextJSTopic = "hello-from-next-js";
        private const string DotNetTopic = "hello-from-dot-net";

        private const string BrokerHost = "rabbitmq";
        private const int BrokerPort = 15674;

        private const string User = "guest";
        private const string Passcode = "guest";

        static async Task Main()
        {
            using IStompClient client = new StompWebsocketClient(new Uri($"ws://{BrokerHost}:{BrokerPort}/ws"));
            //using IStompClient client = new StompTcpClient(BrokerHost, BrokerPort);
            Console.WriteLine("Connecting...");
            await client.Connect(new Dictionary<string, string>()
                {
                    { "User", User },
                    { "Passcode", Passcode },
                });
            Console.WriteLine("Connected.");

           await client.Subscribe<string>($"/topic/{NextJSTopic}", new Dictionary<string, string>(), NextJsSaysHello);

            var timer = new System.Timers.Timer(2000);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(async (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Console.WriteLine("Saying Hello...");
                await client.Send("Hello from dotnet core!!", $"/topic/{DotNetTopic}", new Dictionary<string, string>());
            });
            timer.Start();

            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();
        }

        private static void NextJsSaysHello(object sender, string content)
        {
            Console.WriteLine($"NextJS says: {content}");
        }
    }
}
