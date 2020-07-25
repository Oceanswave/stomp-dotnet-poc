namespace dotnet_core_client.StompClient
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a stomp message
    /// </summary>
    public class StompFrame
    {
        #region Constructors
        /// <summary>
        /// Creates a new Stomp frame of the specified frame kind with an empty body and no headers.
        /// </summary>
        /// <param name="frameKind"></param>
        public StompFrame(string frameKind)
            : this(frameKind, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new Stomp frame of the specified frame kind and message body with no headers.
        /// </summary>
        /// <param name="frameKind"></param>
        /// <param name="body"></param>
        public StompFrame(string frameKind, string body)
            : this(frameKind, body, new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Creates a new Stomp frame of the specified frame kind, empty body and headers.
        /// </summary>
        /// <param name="frameKind"></param>
        /// <param name="headers"></param>
        public StompFrame(string frameKind, IDictionary<string, string> headers)
            : this(frameKind, string.Empty, headers)
        {
        }

        /// <summary>
        /// Creates a new Stomp frame of the specified frame kind, body and headers.
        /// </summary>
        /// <param name="frameKind"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        public StompFrame(string frameKind, string body, IDictionary<string, string> headers)
        {
            FrameKind = frameKind;
            Body = body;
            Headers = headers;
        }
        #endregion

        /// <summary>
        /// Gets or sets the specified header value.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string this[string header]
        {
            get => Headers.ContainsKey(header) ? Headers[header] : string.Empty;
            set => Headers[header] = value;
        }

        /// <summary>
        /// Gets the collection of headers associated with the frame.
        /// </summary>
        public IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the message body of the frame.
        /// </summary>
        /// 
        public string Body { get; }
        /// <summary>
        /// Gets the kind of the frame (SEND, SUBSCRIBE, etc...)
        /// </summary>
        public string FrameKind { get; }
    }
}
