using System.Threading;
using System.Threading.Tasks;

namespace Thiagosza.RabbitMq.Core.Interfaces
{
    public interface IMessageHandler<TMessage>
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken);
    }
}
