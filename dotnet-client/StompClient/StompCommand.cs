﻿namespace dotnet_core_client.StompClient
{
    public static class StompCommand
    {
        //Client Command
        public const string Connect = "CONNECT";
        public const string Disconnect = "DISCONNECT";
        public const string Subscribe = "SUBSCRIBE";
        public const string Unsubscribe = "UNSUBSCRIBE";
        public const string Send = "SEND";

        //Server Response
        public const string Connected = "CONNECTED";
        public const string Message = "MESSAGE";
        public const string Error = "ERROR";
    }
}
