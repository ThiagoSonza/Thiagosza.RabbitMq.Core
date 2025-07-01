using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Thiagosza.RabbitMq.Core.Interfaces;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    internal class MessageHandlerRegistry
    {
        private readonly ConcurrentDictionary<Type, Type> _handlers = new ConcurrentDictionary<Type, Type>();

        internal void Register<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>
            => _handlers[typeof(TMessage)] = typeof(THandler);

        internal void Register(Type messageType, Type handlerType)
            => _handlers[messageType] = handlerType;

        internal Type? GetHandlerType(Type messageType)
            => _handlers.TryGetValue(messageType, out var handlerType) ? handlerType : null;

        internal IEnumerable<(Type MessageType, Type HandlerType)> GetAllRegistrations()
            => _handlers.Select(kvp => (kvp.Key, kvp.Value));
    }
}
