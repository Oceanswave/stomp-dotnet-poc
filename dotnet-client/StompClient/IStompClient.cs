namespace dotnet_core_client.StompClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an interface to a client that connects to a server running the stomp protocol such as RabbitMQ, Apollo or many more. 
    /// </summary>
    public interface IStompClient : IDisposable
    {
        event EventHandler<StompFrame> OnConnected;
        event EventHandler<string> OnDisconnected;
        event EventHandler<string> OnError;

        StompConnectionStatus State { get; }

        /// <summary>
        /// Establishes a connection to the Stomp Websockets client.
        /// </summary>
        /// <param name="headers"></param>
        Task Connect(IDictionary<string, string> headers);

        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="destination"></param>
        /// <param name="headers"></param>
        Task Send<T>(T body, string destination, IDictionary<string, string> headers);

        /// <summary>
        /// Subscribes the client to the specified destination
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination"></param>
        /// <param name="headers"></param>
        /// <param name="handler"></param>
        Task Subscribe<T>(string destination, IDictionary<string, string> headers, EventHandler<T> handler);
    }
}
