namespace dotnet_core_client.StompClient
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a Stomp client that uses a straight Tcp connection to a Stomp server.
    /// </summary>
    public class StompTcpClient : IStompClient
    {
        #region Consts/Default values
        const int DEFAULTTIMEOUT = 5000; //Default to 5 seconds on all timeouts
        const int RECONNECTINTERVAL = 2000; //Default to 2 seconds reconnect attempt rate
        #endregion

        private readonly object _syncRoot = new object();

        public event EventHandler<StompFrame> OnConnected;
        public event EventHandler<string> OnDisconnected;
        public event EventHandler<string> OnError;

        private readonly System.Timers.Timer _receiveTimeout = new System.Timers.Timer();
        private readonly System.Timers.Timer _sendTimeout = new System.Timers.Timer();
        private readonly System.Timers.Timer _connectTimeout = new System.Timers.Timer();

        private readonly string _hostname;
        private readonly int _port;
        private readonly TcpClient _tcpClient;
        private readonly StompMessageSerializer _stompSerializer = new StompMessageSerializer();
        private readonly Encoding _encode = Encoding.Default;

        private readonly IDictionary<string, Subscriber> _subscribers = new Dictionary<string, Subscriber>();

        private IDictionary<string, string> _connectHeaders;
        private Task<StompFrame> _getNextFrameTask = null;

        public StompTcpClient(string hostname, int port, bool autoreconnect = true)
        {
            _hostname = hostname;
            _port = port;
            AutoReconnect = autoreconnect;
            _tcpClient = new TcpClient(AddressFamily.InterNetwork)
            {
                NoDelay = true
            };

            ReceiveTimeout = DEFAULTTIMEOUT;
            SendTimeout = DEFAULTTIMEOUT;
            ConnectTimeout = DEFAULTTIMEOUT;
            ReconnectInterval = RECONNECTINTERVAL;

            _receiveTimeout.AutoReset = false;
            _receiveTimeout.Elapsed += new System.Timers.ElapsedEventHandler(TimerReceiveTimeout_Elapsed);
            _connectTimeout.AutoReset = false;
            _connectTimeout.Elapsed += new System.Timers.ElapsedEventHandler(TimerConnectTimeout_Elapsed);
            _sendTimeout.AutoReset = false;
            _sendTimeout.Elapsed += new System.Timers.ElapsedEventHandler(TimerSendTimeout_Elapsed);
        }

        /// <summary>
        /// Gets the current state of the Stomp TCP Connection.
        /// </summary>
        public StompConnectionStatus State { get; private set; } = StompConnectionStatus.NeverConnected;

        /// <summary>
        /// True to autoreconnect at the given reconnection interval after a remote host closes the connection
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Time to wait after a receive operation is attempted before a timeout event occurs
        /// </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// Time to wait after a send operation is attempted before a timeout event occurs
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// Time to wait after a connection is attempted before a timeout event occurs
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Time to wait before attempting a reconnection
        /// </summary>
        public int ReconnectInterval { get; set; }

        public async Task Connect(IDictionary<string, string> headers)
        {
            if (State == StompConnectionStatus.Connected)
                throw new InvalidOperationException("The stomp client is already connected.");

            _connectHeaders = headers;
            _connectTimeout.Start();
            await _tcpClient.ConnectAsync(_hostname, _port);
            if (!_tcpClient.Connected)
            {
                if (AutoReconnect)
                {
                    Thread.Sleep(ReconnectInterval);
                    // prevent stack overflows
                    var reconnect = new Func<IDictionary<string, string>, Task>(Connect);
                    await reconnect.Invoke(_connectHeaders);
                    return;
                }
                else
                    return;
            }

            // Send the CONNECT frame
            var connectMessage = new StompFrame(StompFrameKind.Connect, headers);
            await _tcpClient.Client.SendAsync(_encode.GetBytes(_stompSerializer.Serialize(connectMessage)), SocketFlags.None);

            // Wait for the connect response
            var responseFrame = await GetNextFrame();
            // Expect that we received a connected response frame
            if (responseFrame.FrameKind != StompFrameKind.Connected)
            {
                State = StompConnectionStatus.Error;
                OnError?.Invoke(this, string.Empty);
                throw new InvalidOperationException($"Expected a server connect response frame, but instead received {responseFrame.FrameKind}");
            }

            _connectTimeout.Stop();
            State = StompConnectionStatus.Connected;
            OnConnected?.Invoke(this, responseFrame);

            var next = new Func<Task<StompFrame>>(GetNextFrame);
            _getNextFrameTask = next.Invoke();
        }

        public async Task Send<T>(T body, string destination, IDictionary<string, string> headers)
        {
            if (State != StompConnectionStatus.Connected)
                throw new InvalidOperationException("The stomp client is not connected.");

            var jsonPayload = JsonConvert.SerializeObject(body);
            headers.Add("destination", destination);
            headers.Add("content-type", "application/json;charset=UTF-8");
            headers.Add("content-length", Encoding.UTF8.GetByteCount(jsonPayload).ToString());
            var message = new StompFrame(StompFrameKind.Send, jsonPayload, headers);

            _sendTimeout.Start();
            await _tcpClient.Client.SendAsync(_encode.GetBytes(_stompSerializer.Serialize(message)), SocketFlags.None);
            _sendTimeout.Stop();
        }

        public Task Subscribe<T>(string topic, IDictionary<string, string> headers, EventHandler<T> handler)
        {
            if (State != StompConnectionStatus.Connected)
                throw new InvalidOperationException("The stomp client is not connected.");

            headers.Add("destination", topic);
            headers.Add("id", "stub"); // todo: study and implement
            var subscribeMessage = new StompFrame(StompFrameKind.Subscribe, headers);
            _tcpClient.Client.Send(_encode.GetBytes(_stompSerializer.Serialize(subscribeMessage)));

            var sub = new Subscriber((sender, body) => handler(this, (T)body), typeof(T));
            _subscribers.Add(topic, sub);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //TODO: Send DISCONNECT frame.

            _tcpClient.Close();
            State = StompConnectionStatus.DisconnectedByUser;
            ((IDisposable)_tcpClient).Dispose();
        }

        #region Private methods
        private void TimerSendTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            State = StompConnectionStatus.SendFail_Timeout;
            DisconnectByHost();
        }

        private void TimerReceiveTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            State = StompConnectionStatus.ReceiveFail_Timeout;
            DisconnectByHost();
        }

        private void TimerConnectTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            State = StompConnectionStatus.ConnectFail_Timeout;
            DisconnectByHost();
        }

        /// <summary>
        /// Waits for a Stomp frame to be received.
        /// </summary>
        /// <returns></returns>
        private async Task<StompFrame> GetNextFrame()
        {
            var responseBuffer = new byte[5000];
            try
            {
                var length = await _tcpClient.Client.ReceiveAsync(responseBuffer, SocketFlags.None);

                // Expect that we receive a response frame
                if (length > 0)
                {
                    var rawResponseFrame = _encode.GetString(responseBuffer);
                    var responseFrame = _stompSerializer.Deserialize(rawResponseFrame);

                    if (responseFrame.FrameKind == StompFrameKind.Message)
                    {
                        var key = responseFrame.Headers["destination"];

                        if (_subscribers.ContainsKey(key))
                        {
                            var sub = _subscribers[key];
                            if (sub.BodyType == typeof(string))
                            {
                                sub.Handler(this, responseFrame.Body);
                            }
                            else
                            {
                                var body = JsonConvert.DeserializeObject(responseFrame.Body, sub.BodyType);
                                sub.Handler(this, body);
                            }
                        }
                    }

                    if (State == StompConnectionStatus.Connected && _getNextFrameTask != null)
                    {
                        var next = new Func<Task<StompFrame>>(GetNextFrame);
                        _getNextFrameTask = next.Invoke();
                    }
                    return responseFrame;
                }
                else
                {
                    State = StompConnectionStatus.Error;
                    OnError?.Invoke(this, string.Empty);
                    throw new InvalidOperationException("Received an empty response frame.");
                }
            }
            catch(SocketException)
            {
                Action doDCHost = new Action(DisconnectByHost);
                doDCHost.Invoke();
                return null;
            }
        }

        private void DisconnectByHost()
        {
            State = StompConnectionStatus.DisconnectedByHost;
            _getNextFrameTask = null;
            _receiveTimeout.Stop();
            if (AutoReconnect)
                Reconnect();
        }

        private void Reconnect()
        {
            if (State == StompConnectionStatus.Connected)
                return;

            State = StompConnectionStatus.AutoReconnecting;
            try
            {
                _tcpClient.Client.Disconnect(true);
                if (AutoReconnect)
                {
                    var doConnect = new Func<IDictionary<string, string>, Task>(Connect);
                    _ = doConnect.Invoke(_connectHeaders);
                    return;
                }

                OnDisconnected?.Invoke(this, string.Empty);
            }
            catch
            {
                // Do Nothing
            }
        }
        #endregion
    }
}
