namespace dotnet_core_client.StompClient
{
    using System;

    public class Subscriber
    {
        public EventHandler<object> Handler { get; }
        public Type BodyType { get; }

        public Subscriber(EventHandler<object> handler, Type bodyType)
        {
            Handler = handler;
            BodyType = bodyType;
        }
    }
}
