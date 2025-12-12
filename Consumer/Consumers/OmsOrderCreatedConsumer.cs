using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Consumer.Config;
using System.Text;
using Common;
using Lab1Try2.BBL.Models;
using Lab1Try2.Clients;
using Lab1Try2.DAL.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics; // Добавлено для Stopwatch

namespace Consumer.Consumers
{
    public class OmsOrderCreatedConsumer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<RabbitMqSettings> _rabbitMqSettings;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private readonly ILogger<OmsOrderCreatedConsumer> _logger; // Добавлено для логирования

        public OmsOrderCreatedConsumer(
            IOptions<RabbitMqSettings> rabbitMqSettings,
            IServiceProvider serviceProvider,
            ILogger<OmsOrderCreatedConsumer> logger) // Добавлен параметр логгера
        {
            _rabbitMqSettings = rabbitMqSettings;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.Value.HostName,
                Port = rabbitMqSettings.Value.Port
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(
                queue: _rabbitMqSettings.Value.OrderCreatedQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var sw = new Stopwatch();

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.ReceivedAsync += async (sender, args) =>
            {
                sw.Restart();
                try
                {
                    var body = args.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var order = message.FromJson<OmsOrderCreatedMessage>();

                    _logger.LogInformation("Received OMS Order Created: {OrderId}", order.Id);
                    Console.WriteLine("Received: " + message);

                    using var scope = _serviceProvider.CreateScope();
                    var client = scope.ServiceProvider.GetRequiredService<OmsClient>();

                    await client.LogOrder(new V1AuditLogOrderRequest
                    {
                        Orders = order.OrderItems.Select(x =>
                            new V1AuditLogOrderRequest.LogOrder
                            {
                                OrderId = order.Id,
                                OrderItemId = x.Id,
                                CustomerId = order.CustomerId,
                                OrderStatus = nameof(OrderStatus.Created)
                            }).ToArray()
                    }, CancellationToken.None);

                    await _channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);

                    sw.Stop();
                    _logger.LogInformation("Order {OrderId} created consumed in {ElapsedMilliseconds} ms",
                        order.Id, sw.ElapsedMilliseconds);
                    Console.WriteLine($"Order created consumed in {sw.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "Error processing OMS order created message");
                    Console.WriteLine(ex.Message);

                    // Отправляем сообщение обратно в очередь для повторной обработки
                    await _channel.BasicNackAsync(args.DeliveryTag, false, true, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _rabbitMqSettings.Value.OrderCreatedQueue,
                autoAck: false, // Автокоммит отключен
                consumer: _consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("OMS Order Created Consumer started listening on queue: {Queue}",
                _rabbitMqSettings.Value.OrderCreatedQueue);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel?.IsOpen ?? false)
            {
                await _channel.CloseAsync();
            }

            if (_connection?.IsOpen ?? false)
            {
                await _connection.CloseAsync();
            }

            _channel?.Dispose();
            _connection?.Dispose();

            _logger.LogInformation("OMS Order Created Consumer stopped");

            await Task.CompletedTask;
        }
    }

    public enum OrderStatus
    {
        Created,
        Processing,
        Completed,
        Cancelled
    }
}