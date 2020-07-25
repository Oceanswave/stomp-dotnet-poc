namespace dotnet_core_client.StompClient
{
    /// <summary>
    /// A collection of the STOMP frames. See https://stomp.github.io/stomp-specification-1.2.html#CONNECT_or_STOMP_Frame
    /// </summary>
    public static class StompFrameKind
    {
        /// <summary>
        /// A STOMP client initiates the stream or TCP connection to the server by sending the CONNECT frame.
        /// </summary>
        public const string Connect = "CONNECT";
        /// <summary>
        /// Indicates a DISCONNECT frame. A client can disconnect from the server at anytime by closing the socket but there is no guarantee that the previously sent frames have been received by the server.
        /// </summary>
        public const string Disconnect = "DISCONNECT";
        /// <summary>
        /// The SUBSCRIBE frame is used to register to listen to a given destination.
        /// </summary>
        public const string Subscribe = "SUBSCRIBE";
        /// <summary>
        /// The UNSUBSCRIBE frame is used to remove an existing subscription.
        /// </summary>
        public const string Unsubscribe = "UNSUBSCRIBE";
        /// <summary>
        /// The SEND frame sends a message to a destination in the messaging system.
        /// </summary>
        public const string Send = "SEND";

        /// <summary>
        /// BEGIN is used to start a transaction.
        /// </summary>
        public const string Begin = "BEGIN";
        /// <summary>
        /// COMMIT is used to commit a transaction in progress.
        /// </summary>
        public const string Commit = "COMMIT";
        /// <summary>
        /// ABORT is used to roll back a transaction in progress.
        /// </summary>
        public const string Abort = "ABORT";
        /// <summary>
        /// ACK is used to acknowledge consumption of a message from a subscription using client or client-individual acknowledgment.
        /// </summary>
        public const string Ack = "ACK";
        /// <summary>
        /// NACK is the opposite of ACK. It is used to tell the server that the client did not consume the message.
        /// </summary>
        public const string Nack = "NACK";

        /// <summary>
        /// A CONNECTED frame is sent from the server to the client if the server accepts the connection attempt.
        /// </summary>
        public const string Connected = "CONNECTED";
        /// <summary>
        /// A RECEIPT frame is sent from the server to the client once a server has successfully processed a client frame that requests a receipt.
        /// </summary>
        public const string Receipt = "RECEIPT";
        /// <summary>
        /// MESSAGE frames are used to convey messages from subscriptions to the client.
        /// </summary>
        public const string Message = "MESSAGE";
        /// <summary>
        /// The server MAY send ERROR frames if something goes wrong. In this case, it MUST then close the connection just after sending the ERROR frame.
        /// </summary>
        public const string Error = "ERROR";
    }
}
