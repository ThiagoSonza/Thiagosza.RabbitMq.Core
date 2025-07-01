using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading;

namespace Thiagosza.RabbitMq.Core.Implementation
{
    internal class MessageDispatcherPolicy
    {
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 2000;

        internal static AsyncRetryPolicy GetPolicy(CancellationToken cancellationToken)
        {
            return Policy
                .Handle<TimeoutException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: MAX_RETRIES,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(RETRY_DELAY_MS, attempt)),
                    onRetry: (exception, timeSpan, attempt, context) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(cancellationToken);

                        Console.WriteLine($"Erro ao processar mensagem. Tentativa {attempt}. Aguardando {timeSpan} para nova tentativa.");
                    });
        }
    }
}
