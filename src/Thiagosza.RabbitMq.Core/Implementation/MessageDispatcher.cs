using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    internal class MessageDispatcher
    {
        private const string METHOD = "HandleAsync";        

        private readonly IServiceProvider _provider;
        private readonly MessageHandlerRegistry _registry;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(
            IServiceProvider provider, 
            MessageHandlerRegistry registry, 
            ILogger<MessageDispatcher> logger)
        {
            _provider = provider;
            _registry = registry;
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
        public async Task DispatchAsync(string messageType, string messageJson, CancellationToken cancellationToken)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.AssemblyQualifiedName == messageType || t.FullName == messageType || t.Name == messageType);

            if (type is null)
            {
                _logger.LogWarning("Tipo {Type} não encontrado", messageType);
                return;
            }

            var handlerType = _registry.GetHandlerType(type);
            if (handlerType == null)
            {
                _logger.LogWarning("Handler para o tipo {Type} não encontrado", type.Name);
                return;
            }

            var handler = _provider.GetRequiredService(handlerType);
            var message = JsonSerializer.Deserialize(messageJson, type);
            var method = handlerType.GetMethod(METHOD);
            if (method == null) return;

            var retryPolicy = MessageDispatcherPolicy.GetPolicy(cancellationToken);
            await retryPolicy.ExecuteAsync(async () => await (Task)(method.Invoke(handler, new[] { message!, cancellationToken }) ?? Task.CompletedTask));
        }
    }
}
