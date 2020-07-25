namespace dotnet_core_client.StompClient
{
    using System;
    using System.Collections.Generic;
    using WebSocketSharp;

    public interface IStompClient : IDisposable
    {
        event EventHandler OnOpen;
        event EventHandler<CloseEventArgs> OnClose;
        event EventHandler<ErrorEventArgs> OnError;

        StompConnectionState State { get; }

        void Connect(IDictionary<string, string> headers);
        void Send(object body, string destination, IDictionary<string, string> headers);
        void Subscribe<T>(string topic, IDictionary<string, string> headers, EventHandler<T> handler);
    }
}
