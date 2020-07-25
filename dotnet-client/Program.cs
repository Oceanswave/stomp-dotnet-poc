using System;
using System.Text;
using Stomp.Net;

namespace dotnet_core_client
{
    class Program
    {
        //TODO: Pull this from environment variables
        private const string NextJSTopic = "hello-from-next-js";
        private const string DotNetTopic = "hello-from-dot-net";

        private const string BrokerUri = "tcp://localhost:61613";

        private const string Password = "guest";
        private const string User = "guest";

        static void Main(string[] args)
        {
            // Configure a logger to capture the output of the library
            Tracer.Trace = new ConsoleLogger();
            Tracer.AddCallerInfo = true;

            // Create a connection factory

            var factory = new ConnectionFactory(BrokerUri,
                                                 new StompConnectionSettings
                                                 {
                                                     UserName = User,
                                                     Password = Password,
                                                     SkipDestinationNameFormatting = false, // Determines whether the destination name formatting should be skipped or not.
                                                     SetHostHeader = true, // Determines whether the host header will be added to messages or not
                                                     HostHeaderOverride = null // Can be used to override the content of the host header
                                                 });

            // Create connection for both requests and responses
            using (var connection = factory.CreateConnection())
            {
                // Open the connection
                connection.Start();

                ListenForText(connection);

                SayHello(connection);
            }
        }

        private static void SayHello(IConnection connection)
        {
            // Create session for responses
            using (var session = connection.CreateSession(AcknowledgementMode.IndividualAcknowledge))
            {
                // Create a message producer
                IDestination destinationQueue = session.GetTopic(DotNetTopic);
                using (var producer = session.CreateProducer(destinationQueue))
                {
                    producer.DeliveryMode = MessageDeliveryMode.Persistent;

                    // Send a message to the destination
                    var message = session.CreateBytesMessage(Encoding.UTF8.GetBytes("Hello from Dot Net!!"));
                    message.StompTimeToLive = TimeSpan.FromMinutes(1);
                    message.Headers["test"] = "test";
                    producer.Send(message);
                    Console.WriteLine("\n\nMessage sent\n");
                }
            }
        }

        private static void ListenForText(IConnection connection)
        {
            // Create session for responses
            using (var session = connection.CreateSession(AcknowledgementMode.IndividualAcknowledge))
            {
                // Create a message consumer
                IDestination sourceQueue = session.GetTopic(NextJSTopic);
                using (var consumer = session.CreateConsumer(sourceQueue))
                {
                    // Wait for a message => blocking call; use consumer.Listener to receive messages as events (none blocking call)
                    var msg = consumer.Receive();

                    var s = Encoding.UTF8.GetString(msg.Content);
                    Console.WriteLine($"\n\nMessage received: {s} from destination: {msg.FromDestination.PhysicalName}");

                    msg.Acknowledge();
                    foreach (var key in msg.Headers.Keys)
                        Console.WriteLine($"\t{msg.Headers[key]}");
                }
            }
        }
    }
}
