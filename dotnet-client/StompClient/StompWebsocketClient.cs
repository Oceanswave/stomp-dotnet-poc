namespace dotnet_core_client.StompClient
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using WebSocketSharp;

    /// <summary>
    /// Represents a Stomp client that uses websockets as the transport layer negotation protocol.
    /// </summary>
    public class StompWebsocketClient : IStompClient
    {
        public event EventHandler<StompFrame> OnConnected;
        public event EventHandler<string> OnDisconnected;
        public event EventHandler<string> OnError;

        private readonly WebSocket _socket;
        private readonly StompMessageSerializer _stompSerializer = new StompMessageSerializer();

        private readonly IDictionary<string, Subscriber> subscribers = new Dictionary<string, Subscriber>();

        public StompWebsocketClient(Uri brokerUri)
        {
            _socket = new WebSocket(brokerUri.ToString());
        }

        public StompConnectionStatus State { get; private set; } = StompConnectionStatus.NeverConnected;

        public Task Connect(IDictionary<string, string> headers)
        {
            if (State != StompConnectionStatus.NeverConnected && State != StompConnectionStatus.DisconnectedByHost && State != StompConnectionStatus.DisconnectedByUser)
                throw new InvalidOperationException("The current state of the connection is not Closed.");

            State = StompConnectionStatus.Connecting;
            _socket.Connect();

            var connectMessage = new StompFrame(StompFrameKind.Connect, headers);
            _socket.Send(_stompSerializer.Serialize(connectMessage));

            _socket.OnMessage += HandleMessage;
            _socket.OnError += (object sender, ErrorEventArgs e) =>
            {
                State = StompConnectionStatus.Error;
                OnError?.Invoke(this, e.Message);
            };

            _socket.OnClose += (object sender, CloseEventArgs e) =>
            {
                State = StompConnectionStatus.DisconnectedByHost;
                OnDisconnected?.Invoke(this, e.Reason);
            };

            _socket.OnOpen += (object sender, EventArgs e) =>
            {
                State = StompConnectionStatus.Connected;
                OnConnected?.Invoke(this, null);
            };

            State = StompConnectionStatus.Connected;
            return Task.CompletedTask;
        }

        public Task Send<T>(T body, string destination, IDictionary<string, string> headers)
        {
            if (State != StompConnectionStatus.Connected)
                throw new InvalidOperationException("The stomp client is not connected.");

            var jsonPayload = JsonConvert.SerializeObject(body);
            headers.Add("destination", destination);
            headers.Add("content-type", "application/json;charset=UTF-8");
            headers.Add("content-length", Encoding.UTF8.GetByteCount(jsonPayload).ToString());
            var connectMessage = new StompFrame(StompFrameKind.Send, jsonPayload, headers);
            _socket.Send(_stompSerializer.Serialize(connectMessage));
            return Task.CompletedTask;
        }

        public Task Subscribe<T>(string topic, IDictionary<string, string> headers, EventHandler<T> handler)
        {
            if (State != StompConnectionStatus.Connected)
                throw new InvalidOperationException("The stomp client is not connected.");

            headers.Add("destination", topic);
            headers.Add("id", "stub"); // todo: study and implement
            var subscribeMessage = new StompFrame(StompFrameKind.Subscribe, headers);
            _socket.Send(_stompSerializer.Serialize(subscribeMessage));

            var sub = new Subscriber((sender, body) => handler(this, (T)body), typeof(T));
            subscribers.Add(topic, sub);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //TODO: Send DISCONNECT frame.

            _socket.Close();
            State = StompConnectionStatus.DisconnectedByUser;
            ((IDisposable)_socket).Dispose();
        }

        private void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = _stompSerializer.Deserialize(messageEventArgs.Data);
            if (message.FrameKind == StompFrameKind.Message)
            {
                var key = message.Headers["destination"];

                if (subscribers.ContainsKey(key))
                {
                    var sub = subscribers[key];
                    if (sub.BodyType == typeof(string))
                    {
                        sub.Handler(sender, message.Body);
                    }
                    else
                    {
                        var body = JsonConvert.DeserializeObject(message.Body, sub.BodyType);
                        sub.Handler(sender, body);
                    }
                }
            }
        }
    }
}
