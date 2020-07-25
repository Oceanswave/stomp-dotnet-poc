namespace dotnet_core_client.StompClient
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using WebSocketSharp;

    public class StompWebsocketClient : IStompClient
    {
        public event EventHandler OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;
        public event EventHandler<ErrorEventArgs> OnError;

        private readonly Uri _brokerUri;
        private readonly WebSocket _socket;
        private readonly StompMessageSerializer _stompSerializer = new StompMessageSerializer();

        private readonly IDictionary<string, Subscriber> subscribers = new Dictionary<string, Subscriber>();

        public StompConnectionState State { get; private set; } = StompConnectionState.Closed;

        public StompWebsocketClient(Uri brokerUri)
        {
            _brokerUri = brokerUri;
            _socket = new WebSocket(brokerUri.ToString());
        }

        public void Connect(IDictionary<string, string> headers)
        {
            if (State != StompConnectionState.Closed)
                throw new InvalidOperationException("The current state of the connection is not Closed.");

            _socket.Connect();

            var connectMessage = new StompMessage(StompCommand.Connect, headers);
            _socket.Send(_stompSerializer.Serialize(connectMessage));

            _socket.OnMessage += HandleMessage;
            _socket.OnError += OnError;
            _socket.OnClose += OnClose;
            _socket.OnOpen += OnOpen;

            State = StompConnectionState.Open;
        }

        public void Send(object body, string destination, IDictionary<string, string> headers)
        {
            if (State != StompConnectionState.Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            var jsonPayload = JsonConvert.SerializeObject(body);
            headers.Add("destination", destination);
            headers.Add("content-type", "application/json;charset=UTF-8");
            headers.Add("content-length", Encoding.UTF8.GetByteCount(jsonPayload).ToString());
            var connectMessage = new StompMessage(StompCommand.Send, jsonPayload, headers);
            _socket.Send(_stompSerializer.Serialize(connectMessage));
        }

        public void Subscribe<T>(string topic, IDictionary<string, string> headers, EventHandler<T> handler)
        {
            if (State != StompConnectionState.Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            headers.Add("destination", topic);
            headers.Add("id", "stub"); // todo: study and implement
            var subscribeMessage = new StompMessage(StompCommand.Subscribe, headers);
            _socket.Send(_stompSerializer.Serialize(subscribeMessage));
            // todo: check response
            // todo: implement advanced topic
            var sub = new Subscriber((sender, body) => handler(this, (T)body), typeof(T));
            subscribers.Add(topic, sub);
        }

        public void Dispose()
        {
            if (State != StompConnectionState.Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            _socket.Close();
            State = StompConnectionState.Closed;
            ((IDisposable)_socket).Dispose();
            // todo: unsubscribe
        }

        private void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = _stompSerializer.Deserialize(messageEventArgs.Data);
            if (message.Command == StompCommand.Message)
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
