using System;

namespace Thiagosza.RabbitMq.Core
{
    internal class MessageWrapper<TMessage>
    {
        public string MessageType => typeof(TMessage).FullName;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TMessage Payload { get; set; } = default!;

        internal static object Create(TMessage message)
        {
            return new MessageWrapper<TMessage>
            {
                Payload = message,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}


