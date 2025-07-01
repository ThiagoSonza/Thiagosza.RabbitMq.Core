using System.Threading;
using System.Threading.Tasks;

namespace Thiagosza.RabbitMq.Core.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
    }
}
