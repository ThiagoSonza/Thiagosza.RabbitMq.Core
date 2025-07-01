using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MessageDispatcher _dispatcher;
        private readonly RabbitMqMessagingOptions _options;

        private IConnection _connection = default!;
        private readonly IList<IChannel> _channels = new List<IChannel>();

        public Worker(ILogger<Worker> logger, MessageDispatcher dispatcher, RabbitMqMessagingOptions options)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _options = options;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = await _options.ConnectionFactory.CreateConnectionAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            foreach (var (consumerType, queueName) in _options.ConsumerBindings)
            {
                var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                _channels.Add(channel);

                await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var messageType = ea.BasicProperties.Type;

                    if (string.IsNullOrWhiteSpace(messageType))
                    {
                        _logger.LogWarning("Tipo da mensagem não especificado.");
                        return;
                    }

                    await _dispatcher.DispatchAsync(messageType, json, cancellationToken);
                };

                await channel!.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var channelsOpen = _channels.Where(q => q.IsOpen).ToList();
            foreach(var channel in channelsOpen)
                await channel.CloseAsync(cancellationToken: cancellationToken);

            if (_connection != null && _connection.IsOpen)
                await _connection.CloseAsync(cancellationToken: cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
