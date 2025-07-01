using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    public class RabbitMqMessagingOptions
    {
        /// <summary>
        /// Lista de bindings de consumidores, onde cada tupla contém o tipo do consumidor e o nome da fila.
        /// </summary>
        /// <remarks>
        /// Esta lista é utilizada para registrar os consumidores e as filas que eles irão consumir.
        /// Cada consumidor deve ser uma classe que implementa a interface IConsumer.
        /// O nome da fila deve ser único para cada consumidor.
        /// </remarks>
        public List<(Type ConsumerType, string QueueName)> ConsumerBindings { get; } = new List<(Type ConsumerType, string QueueName)>();

        /// <summary>
        /// Lista de bindings de produtores, onde cada tupla contém o tipo do produtor e o nome da fila.
        /// </summary>
        /// <remarks>
        /// Esta lista é utilizada para registrar os produtores e as filas que eles irão produzir.
        /// Cada produtor deve ser uma classe que implementa a interface IProducer.
        /// O nome da fila deve ser único para cada produtor.
        /// </remarks>
        public List<(Type ProducerType, string QueueName)> ProducerBindings { get; } = new List<(Type ProducerType, string QueueName)>();

        /// <summary>
        /// Configuração da fábrica de conexões do RabbitMQ.
        /// </summary>
        /// <remarks>
        /// Esta propriedade é utilizada para configurar a conexão com o RabbitMQ.
        /// Você pode configurar o host, porta, credenciais, etc.
        /// </remarks>
        public ConnectionFactory ConnectionFactory { get; private set; } = new ConnectionFactory();

        /// <summary>
        /// Adiciona um consumidor à lista de bindings.
        /// </summary>
        /// <typeparam name="TConsumer">O tipo do consumidor, que deve implementar a interface IConsumer.</typeparam>
        /// <param name="queueName">O nome da fila que o consumidor irá consumir.</param>
        /// <remarks>
        /// Esta função é utilizada para registrar um consumidor e a fila que ele irá consumir.
        /// Cada consumidor deve ser uma classe que implementa a interface IConsumer.
        /// O nome da fila deve ser único para cada consumidor.
        /// </remarks>
        public void AddConsumer<TConsumer>(string queueName)
            => ConsumerBindings.Add((typeof(TConsumer), queueName));

        /// <summary>
        /// Adiciona um produtor à lista de bindings.
        /// </summary>
        /// <typeparam name="TProducer">O tipo do produtor, que deve implementar a interface IProducer.</typeparam>
        /// <param name="queueName">O nome da fila que o produtor irá produzir.</param>
        /// <remarks>
        /// Esta função é utilizada para registrar um produtor e a fila que ele irá produzir.
        /// Cada produtor deve ser uma classe que implementa a interface IProducer.
        /// O nome da fila deve ser único para cada produtor.
        /// </remarks>
        public void AddProducer<TProducer>(string queueName)
            => ProducerBindings.Add((typeof(TProducer), queueName));

        /// <summary>
        /// Configura o host e a porta do RabbitMQ.
        /// </summary>
        /// <param name="uri">A URI do RabbitMQ, que deve conter o host e a porta.</param>
        /// <param name="port">A porta do RabbitMQ.</param>
        /// <param name="configureFactory">A ação para configurar a fábrica de conexões.</param>
        /// <remarks>
        /// Esta função é utilizada para configurar a conexão com o RabbitMQ.
        /// Você pode configurar o host, porta, credenciais, etc.
        /// O URI deve conter o host e a porta do RabbitMQ.
        /// </remarks>
        public void Host(Uri uri, int port, Action<ConnectionFactory> configureFactory)
        {
            var factory = new ConnectionFactory
            {
                HostName = uri.Host,
                Port = port,
            };

            configureFactory(factory);
            ConnectionFactory = factory;
        }
    }
}
