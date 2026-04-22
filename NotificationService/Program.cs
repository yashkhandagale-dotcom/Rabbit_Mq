using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};

const string exchangeName = "order_exchange";

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Fanout,
    durable: true
);

var queueResult = await channel.QueueDeclareAsync(
    queue: "notification_queue",
    durable: true,
    exclusive: false,
    autoDelete: false
);

await channel.QueueBindAsync(queueResult.QueueName, exchangeName, routingKey: "");
await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"🔔 Notification Sent for: {message}");
    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
};

await channel.BasicConsumeAsync(queueResult.QueueName, autoAck: false, consumer: consumer);

Console.WriteLine("🔔 NotificationService waiting for messages... (Ctrl+C to exit)");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
try { await Task.Delay(Timeout.Infinite, cts.Token); } catch (TaskCanceledException) { }

Console.WriteLine("🔔 NotificationService shutting down.");