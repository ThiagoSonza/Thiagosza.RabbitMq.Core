using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Thiagosza.RabbitMq.Core.Implementation;
using Thiagosza.RabbitMq.Core.Interfaces;

namespace Thiagosza.RabbitMq.Core.Extensions
{
    public static class RabbitMqMessagingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers RabbitMQ messaging services in the specified <see cref="IServiceCollection"/>.
        /// /// This method allows you to configure RabbitMQ messaging options and register message handlers.
        /// </summary>
        /// <param name="services">The service collection to register the services in.</param>
        /// <param name="configure">An optional action to configure RabbitMQ messaging options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a consumer does not implement the required interface.</exception>
        /// <remarks>
        /// This method registers each consumer type as a transient service.
        /// It also registers a singleton <see cref="RabbitMqMessagingOptions"/> to hold configuration settings.
        /// Finally, it adds a hosted service <see cref="Worker"/> to process messages from RabbitMQ.
        /// </remarks>
        public static IServiceCollection AddRabbitMqMessaging(
            this IServiceCollection services,
            Action<RabbitMqMessagingOptions>? configure = null)
        {
            var options = new RabbitMqMessagingOptions();
            configure?.Invoke(options);

            foreach (var (consumerType, queueName) in options.ConsumerBindings)
            {
                var iface = consumerType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType
                                      && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                    ?? throw new InvalidOperationException($"O consumer {consumerType.Name} não implementa IMessageHandler<T>");

                services.AddTransient(consumerType);
            }

            services.AddSingleton(options);
            services.AddSingleton<MessageDispatcher>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHostedService<Worker>();

            return services;
        }
    }
}
