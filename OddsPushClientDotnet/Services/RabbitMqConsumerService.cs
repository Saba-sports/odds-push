using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OddsPushClient.Consumers;

namespace OddsPushClient.Services;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly RawMessageConsumer _consumer;

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IConfiguration configuration,
        RawMessageConsumer consumer)
    {
        _logger = logger;
        _configuration = configuration;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMqConsumerService starting...");

        var rabbitConfig = _configuration.GetSection("OddsPush");
        var amqpConnectionString = rabbitConfig["Connection"] ?? throw new ArgumentNullException("Connection path missing");

        var factory = new ConnectionFactory { Uri = new Uri(amqpConnectionString) };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var queueName = "odds-push-client-queue-please-change-name";
        var exchangeName = rabbitConfig["ExchangeName"] ?? throw new ArgumentNullException("ExchangeName missing");

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: stoppingToken);

        // 綁定訊息
        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: "#", // 此處使用 "#" 萬用匹配，應根據實際生產環境使用情境進行資料過濾
            cancellationToken: stoppingToken);

        _logger.LogInformation("Waiting for messages from exchange: {Exchange}", exchangeName);

        var eventConsumer = new AsyncEventingBasicConsumer(channel);
        eventConsumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                await _consumer.HandleMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message");
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: eventConsumer,
            cancellationToken: stoppingToken);

        // 持續執行直到程式停止
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("RabbitMqConsumerService stopping...");
    }
}
