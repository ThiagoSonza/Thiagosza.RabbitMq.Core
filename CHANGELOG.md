# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/)
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

---

## [1.1.0] - 2025-08-19

- Atualiza a implementação do RabbitMq introduzindo a classe MessageWrapper para encapsular mensagens.
- Ajuste na lógica de publicação e despacho de mensagens

## [1.0.0] - 2025-06-26

- Suporte a consumo de mensagens via `IMessageHandler<T>`
- Registro automático de consumers com fila nomeada
- Registro de producers com resolução automática de fila via tipo da mensagem
- Implementação de `IRabbitMqPublisher` com publish baseado em tipo
- Mapeamento interno de tipo → handler via `MessageHandlerRegistry`
- Retry com backoff exponencial via Polly (tentativas: 3 / delays: 2s, 4s, 8s)
- Suporte à injeção de dependência via `AddRabbitMqMessaging(...)`
- Suporte a `AddHostedService<Worker>()` para background worker
- Compatível com WebAPI (`WebApplication`) com uso simultâneo de REST e mensageria
- Abstração clara entre consumer, dispatcher e publisher

---

## [Unreleased]

### Planejado

- Fallback de mensagens com falha permanente para fila morta
