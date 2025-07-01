# RabbitMQ Messaging

Este projeto fornece uma abstração simples e extensível para consumir e publicar mensagens no RabbitMQ em aplicações .NET (incluindo `Worker`, `WebAPI` ou projetos híbridos). Ele segue um padrão similar ao MassTransit, mas sem dependências externas ou licenciamento pago.

---

## ✨ Recursos

- Registro automático de consumers via reflection
- Definição explícita de fila por consumer
- Publisher desacoplado que resolve a fila com base no tipo da mensagem
- Suporte a retry com delay e backoff exponencial
- Compatível com `AddHostedService<T>()` ou APIs web com `WebApplication`

---

## 📦 Instalação

```bash
dotnet add package Thiagosza.RabbitMq.Core
```

Ou edite seu .csproj:
```bash
<PackageReference Include="Thiagosza.RabbitMq.Core" Version="1.0.0" />
```

## 🚀 Exemplo de Uso

Registre os consumers e producers

```csharp
builder.Services.AddRabbitMqMessaging(x =>
{
    x.Host(new Uri("amqp://localhost"), 5672, h =>
    {
        h.UserName = "guest";
        h.Password = "guest";
        h.DispatchConsumersAsync = true;
    });

    x.AddConsumer<MyMessageHandler>("queue_to_consume");

    x.AddProducer<MyModel>("queue_to_producer");
});
```

Consumindo mensagens
```csharp
public class MyModel
{
    public int Id { get; set; }
    public string Text { get; set; }
}

public class MyMessageHandler : IMessageHandler<MyModel>
{
    public Task HandleAsync(MyModel message, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Message {idMessage} {Text} processed", message.Id, message.Text);
        return Task.CompletedTask;
    }
}
```

Publicando mensagens
```csharp
public class MyModel
{
    public int Id { get; set; }
    public string Text { get; set; }
}

[HttpPost]
public async Task<IActionResult> PublishMessage([FromServices] IRabbitMqPublisher publisher)
{
    await publisher.PublishAsync(new MyModel { Id = 1, Text = "Text to message" });
    return Ok();
}
```