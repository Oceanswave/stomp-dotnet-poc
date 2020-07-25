namespace dotnet_core_client.StompClient
{
    /// <summary>
    /// Indicates the status of the Stomp connection
    /// </summary>
    public enum StompConnectionStatus
    {
        NeverConnected,
        Connecting,
        Connected,
        AutoReconnecting,
        DisconnectedByUser,
        DisconnectedByHost,
        ConnectFail_Timeout,
        ReceiveFail_Timeout,
        SendFail_Timeout,
        SendFail_NotConnected,
        Error
    }
}
