using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Thiagosza.RabbitMq.Core.Interfaces;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqMessagingOptions _options;

        public RabbitMqPublisher(RabbitMqMessagingOptions options)
        {
            _options = options;
            _factory = options.ConnectionFactory;
        }

        /// <summary>
        /// Publishes a message to the RabbitMQ queue.
        /// The queue name is determined by the message type based on the configured producer bindings.
        /// Throws an exception if no queue is configured for the message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
        /// <param name="message">The message instance to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no queue is configured for the message type.</exception>
        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            var messageType = typeof(TMessage);
            var queueName = _options.ProducerBindings.FirstOrDefault(q => q.ProducerType == messageType).QueueName 
                ?? throw new InvalidOperationException($"Fila não configurada para o tipo de mensagem {messageType.FullName}");
            
            using var connection = await _factory.CreateConnectionAsync(cancellationToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                Type = typeof(TMessage).Name,
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
    }
}
