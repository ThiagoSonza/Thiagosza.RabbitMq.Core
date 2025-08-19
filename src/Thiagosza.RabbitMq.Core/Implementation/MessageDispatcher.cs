using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Thiagosza.RabbitMq.Core.Interfaces;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    internal class MessageDispatcher
    {
        private const string METHOD = "HandleAsync";

        private readonly IServiceProvider _provider;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(
            IServiceProvider provider,
            ILogger<MessageDispatcher> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        /// <summary>
        /// Dispatches a message to the appropriate handler based on its type.
        /// The message type is specified by its fully qualified name or assembly qualified name.
        /// The message content is expected to be in JSON format.
        /// </summary>
        /// <param name="messageType">The type of the message to dispatch, specified as a string.</param>
        /// <param name="messageJson">The JSON representation of the message to dispatch.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DispatchAsync(Type handlerType, string messageJson, CancellationToken cancellationToken)
        {
            var handler = _provider.GetService(handlerType);
            if (handler is null)
            {
                _logger.LogError("Handler not injected for type: {handlerName}", handlerType.Name);
                return;
            }

            var method = handlerType.GetMethod(METHOD);
            if (method == null) return;

            var interfaceType = handlerType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

            var genericType = interfaceType.GetGenericArguments().First();
            _logger.LogDebug("Handler: {handlerName}, Interface: {interfaceName}, Generic: {genericName}", handlerType.Name, interfaceType.Name, genericType.Name);

            var json = JsonNode.Parse(messageJson)?.AsObject()!;
            var message = json["Payload"].Deserialize(genericType);

            var retryPolicy = MessageDispatcherPolicy.GetPolicy(cancellationToken);
            await retryPolicy.ExecuteAsync(async () => await (Task)(method.Invoke(handler, new[] { message!, cancellationToken }) ?? Task.CompletedTask));
        }
    }
}
